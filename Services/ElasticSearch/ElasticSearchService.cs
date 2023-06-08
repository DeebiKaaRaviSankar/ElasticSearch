using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Nest;
using Elasticsearch.Net;
using QueryEditor.Models;
using QueryEditor.Models.Search;
using System.Linq.Expressions;
using System.Reflection;
using ElasticSearchPatchGenaration.Models;

namespace QueryEditor.Services.ElasticSearch
{
    public class ElasticSearchService
    {
        protected IProperties IndexMapping { get; set; }

        private const string ElasticSearchClusterUri = "http://localhost:9200";

        private const string IndexNameCustomers = "18455014ef7f413c8be61acca225d24e-customers-test5";

        private static ConnectionSettings GetElasticSearchConnectionSettings(StaticConnectionPool connectionPool) => new ConnectionSettings(connectionPool)
                                                  .DisableDirectStreaming()
                                                  .BasicAuthentication(username: "elastic", password: "pass@123")
                                                  .DefaultIndex(IndexNameCustomers)
                                                  .DisablePing();

        public enum PropertyType
        {
            Nested,
            Object,
            Native
        }

        public class PropertyMetadata
        {
            public string Path;
            public PropertyType Type;
            public IEnumerable<PropertyMetadata> Children;

            public PropertyMetadata()
            {
                this.Path = string.Empty;
                this.Type = PropertyType.Native;
                this.Children = new List<PropertyMetadata> { };
            }

            public static IEnumerable<PropertyMetadata> GetParentPropertyTypeInfo(
                string path,
                PropertyMetadata propertyTypeInfo)
            {
                if (string.IsNullOrEmpty(path))
                {
                    return null;
                }

                var parts = path.Split('.').ToList();
                var propertyInfos = new List<PropertyMetadata> { };

                string currentPath = string.Empty;
                var parent = propertyTypeInfo;
                var properties = PropertyMetadata.GetNestedAndObjectFieldTypes().ToList();

                foreach (var part in parts)
                {
                    currentPath = (string.IsNullOrEmpty(currentPath) ? currentPath : currentPath + ".") + part;

                    var propertyInfo = properties != null
                        ? properties.Find((_) => _.Path == currentPath)
                        : null;

                    if (propertyInfo == null)
                    {
                        break;
                    }

                    parent = propertyInfo;
                    propertyInfos.Add(parent);
                    if (propertyInfo.Type == PropertyType.Nested)
                    {
                        properties = parent.Children.ToList();

                        if (properties == null)
                        {
                            return propertyInfos;
                        }
                    }

                }

                return propertyInfos;
            }

            public static PropertyMetadata GetPropertyTypeInfo(string path)
            {
                if (string.IsNullOrEmpty(path))
                {
                    return null;
                }

                var parts = path.Split('.').ToList();
                var iteratorIndex = 0;
                var part = string.Empty;
                string currentPath = string.Empty;
                var parent = new PropertyMetadata { };
                var properties = PropertyMetadata.GetNestedAndObjectFieldTypes().ToList();
                var result = new PropertyMetadata { };

                do
                {
                    part = parts[iteratorIndex];
                    parent = properties.FirstOrDefault((_) => _.Path == part);
                    if (parent == null || parent.Type != PropertyType.Nested || parent.Type != PropertyType.Object)
                    {
                        return new PropertyMetadata
                        {
                            Path = part,
                            Type = PropertyType.Native
                        };
                    }
                    else
                    {
                        return parent;
                    }
                    iteratorIndex++;
                } while (parent != null);

                return result;
            }

            public static IEnumerable<PropertyMetadata> GetNestedAndObjectFieldTypes()
            {
                return new List<PropertyMetadata>
                {
                    new PropertyMetadata
                    {
                        Path = "contacts",
                        Type = PropertyType.Nested,
                        Children = new List<PropertyMetadata>
                        {
                            new PropertyMetadata
                            {
                                Path = "contacts.customFields",
                                Type = PropertyType.Object,
                                Children = new List<PropertyMetadata>
                                {
                                    new PropertyMetadata
                                    {
                                        Path = "contacts.customFields.texts",
                                        Type = PropertyType.Nested,
                                    },
                                    new PropertyMetadata
                                    {
                                        Path = "contacts.customFields.numbers",
                                        Type = PropertyType.Nested,
                                    },
                                },
                            },
                            new PropertyMetadata
                            {
                                Path = "contacts.campaigns",
                                Type = PropertyType.Nested,
                                Children = new List<PropertyMetadata>
                                {
                                    new PropertyMetadata{
                                        Path = "contacts.campaigns.response",
                                        Type = PropertyType.Object
                                    }
                                }
                            },
                        },
                    },
                    new PropertyMetadata
                    {
                        Path = "opportunities",
                        Type = PropertyType.Nested,
                    },
                    new PropertyMetadata
                    {
                        Path = "account",
                        Type = PropertyType.Object,
                        Children = new List<PropertyMetadata>
                        {
                            new PropertyMetadata
                            {
                                Path = "account.subscriptions",
                                Type = PropertyType.Nested,
                                Children = new List<PropertyMetadata>
                                {
                                    new PropertyMetadata
                                    {
                                        Path = "account.subscriptions.customFields",
                                        Type = PropertyType.Object,
                                        Children = new List<PropertyMetadata>
                                        {
                                            new PropertyMetadata
                                            {
                                                Path = "account.subscriptions.customFields.texts",
                                                Type = PropertyType.Nested,
                                            },
                                            new PropertyMetadata
                                            {
                                                Path = "account.subscriptions.customFields.numbers",
                                                Type = PropertyType.Nested,
                                            },
                                        },
                                    },
                                    new PropertyMetadata
                                    {
                                        Path = "account.subscriptions.invoices",
                                        Type = PropertyType.Nested,
                                    },
                                }
                            },

                        },
                    },
                };
            }
        }

        public static ICreateIndexRequest DescribeIndex(CreateIndexDescriptor indexDescriptor)
        {
            if (indexDescriptor is null)
            {
                throw new ArgumentNullException(nameof(indexDescriptor));
            }

            indexDescriptor.Settings(s => s.Analysis(a => a
                .Normalizers(n => n.Custom("case_insensitive", c => c.Filters("lowercase", "asciifolding")))
                .Analyzers(n => n.Custom("standard", c => c.Filters("lowercase", "asciifolding").Tokenizer("standard"))))
                .Setting(
                    "index.mapping.total_fields.limit",
                    1000));

            return indexDescriptor.Map<CustomerSearch>(x => x
                .Properties(property => property
                .Nested<CustomerContact>(nestedProperty => nestedProperty
                .Name(name => name.Contacts)
                .AutoMap()))
            .Properties(property => property
                .Nested<CustomerOpportunities>(nestedProperty => nestedProperty
                .Name(name => name.Opportunities)
                .AutoMap()))
            .Properties(property => property
                .Object<CustomerAccountSummary>(selector => selector
                    .Name(_ => _.Account)
                    .Properties(subscriptionsProperty => subscriptionsProperty
                        .Nested<SubscriptionSummary>(nestedProperty => nestedProperty
                            .Name(name => name.Subscriptions)
                            .Properties(invoicesProperty => invoicesProperty
                                .Nested<InvoiceSummary>(nestedProperty => nestedProperty
                                    .Name(name => name.Invoices)
                                    .AutoMap()))
                            .AutoMap()))
                    .AutoMap()))
            .AutoMap());
        }

        public static async Task InitializeIndexAsync(ElasticClient elasticClient)
        {
            var existsResult = await elasticClient
                   .Indices
                   .ExistsAsync(IndexNameCustomers)
                   .ConfigureAwait(false);

            if (existsResult.Exists)
            {
                return;
            }

            var result = await elasticClient
                   .Indices
                   .CreateAsync(
                        IndexNameCustomers,
                        DescribeIndex)
                   .ConfigureAwait(false);

            if (result.IsValid == false)
            {
                var exception = result.OriginalException;

                throw new Exception("Could not create index.", exception);
            }
        }

        public static ElasticClient GetElasticClient()
        {
            ElasticClient elasticClient;
            StaticConnectionPool connectionPool;

            var nodes = new Uri[] { new Uri(ElasticSearchClusterUri) };

            connectionPool = new StaticConnectionPool(nodes);
            elasticClient = new ElasticClient(GetElasticSearchConnectionSettings(connectionPool));

            return elasticClient;
        }

        public Dictionary<string, string> GetFieldTypes(string path)
        {
            var parts = path.Split('.').ToList();
            Dictionary<string, string> fieldTypes = new Dictionary<string, string>();
            var firstPart = parts[0];

            IProperties properties = this.IndexMapping;

            if (properties.ContainsKey(firstPart))
            {
                var propertyInfo = properties[firstPart];
                if (propertyInfo.Type == "nested" || propertyInfo.Type == "object")
                {
                    parts.ForEach((part) =>
                    {
                        var isFirstPart = part == firstPart;
                        var isNotLastPart = part != parts[parts.Count - 1];

                        if (isFirstPart || isNotLastPart)
                        {
                            properties = (Properties)propertyInfo
                                            .GetType()
                                            .GetProperty("Properties")
                                            .GetValue(propertyInfo, null);
                        }

                        fieldTypes.Add(part, propertyInfo != null ? propertyInfo.Type : "unknown");
                        if (isNotLastPart)
                        {
                            var propertiesDictionary = (IDictionary<PropertyName, IProperty>)properties;
                            var key = parts[parts.IndexOf(part) + 1];

                            propertyInfo = propertiesDictionary.ContainsKey(key)
                                            ? properties[key]
                                            : null;
                        }
                    });
                }
            }
            ////else
            ////{
            ////    throw new ArgumentException($"Invalid path - {firstPart}. The path does not exist in index");
            ////}

            return fieldTypes;
        }

        public static void GetPropertyName<T>()
        {
            var type = typeof(T);
            var property = type.GetProperties();
            property.ToList().ForEach((propInfo) =>
            {
                var prop = propInfo;
                var propType = prop.PropertyType.Name;
                var baseType = propType == "Object"
                                    || (prop.PropertyType != null
                                         && prop.PropertyType.BaseType == null) ? null : prop.PropertyType.BaseType.Name;
                var name = nameof(baseType);
                var propName = prop.Name;
            });
        }

        ////public virtual async Task<IEnumerable<CustomerSearch>> SearchThroughNestedObjectsAsync(
        ////   ElasticClient elasticClient,
        ////   SearchRequest searchRequest,
        ////   string nestedPath)
        ////{
        ////    if (string.IsNullOrEmpty(nestedPath))
        ////    {
        ////        throw new ArgumentNullException(nestedPath);
        ////    }

        ////    var request = this.ConstructSearchRequest(searchRequest);

        ////    var searchResponse = await elasticClient
        ////        .SearchAsync<CustomerSearch>(request)
        ////        .ConfigureAwait(false);

        ////    var searchResultsWithOnlyChildren = new List<object>();

        ////    var customers = new List<CustomerSearch>();

        ////    var hits = searchResponse.Hits;

        ////    foreach (var hit in hits)
        ////    {
        ////        var parent = hit.Source;
        ////        var children = new List<object>();

        ////        // to do check this code
        ////        foreach (var innerHit in hit.InnerHits)
        ////        {
        ////            if (innerHit.Key == nestedPath)
        ////            {
        ////                var matches = innerHit.Value.Hits.Hits.Select(document => document.Source.As<object>());
        ////                children.AddRange(matches);
        ////            }
        ////        }

        ////        customers.Add(new SearchThroughNestedObjectResult<T>(parent, JsonConvert.SerializeObject(children)));
        ////        searchResultsWithOnlyChildren.AddRange(children);
        ////    }

        ////    return customers;
        ////}
    }
}
