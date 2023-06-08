using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearchPatchGenaration.Models
{
    public enum PropertyType
    {
        Nested,
        Object,
        Native,
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

        public static IEnumerable<PropertyMetadata> GetParentsMetadata(
            string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            var parts = path.Split('.').ToList();
            var propertyInfo = new List<PropertyMetadata> { };

            string currentPath = string.Empty;
            var parent = new PropertyMetadata();
            var properties = PropertyMetadata.GetNestedAndObjectFieldTypes().ToList();

            foreach (var part in parts)
            {
                currentPath = (string.IsNullOrEmpty(currentPath) ? currentPath : currentPath + ".") + part;

                var currentPropertyInfo = properties != null
                    ? properties.Find(
                        (_) => _.Path == currentPath)
                    : null;

                if (currentPropertyInfo == null)
                {
                    propertyInfo.Add(new PropertyMetadata
                    {
                        Path = currentPath,
                        Type = PropertyType.Native,
                    });
                    break;
                }

                parent = currentPropertyInfo;
                propertyInfo.Add(parent);
                if (currentPropertyInfo.Type == PropertyType.Nested || currentPropertyInfo.Type == PropertyType.Object)
                {
                    properties = parent.Children != null || parent.Children.Any() ? parent.Children.ToList() : null;

                    if (properties == null || !properties.Any())
                    {
                        return propertyInfo;
                    }
                }
            }

            return propertyInfo;
        }

        public static PropertyMetadata GetPropertyMetadata(string path)
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
                        Type = PropertyType.Native,
                    };
                }
                else
                {
                    return parent;
                }
            }
            while (parent != null);
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
                                        Type = PropertyType.Nested
                                    },
                                    new PropertyMetadata
                                    {
                                        Path = "contacts.customFields.numbers",
                                        Type = PropertyType.Nested,
                                    }
                                },
                            },
                            new PropertyMetadata
                            {
                                Path = "contacts.campaigns",
                                Type = PropertyType.Nested,
                                Children = new List<PropertyMetadata>
                                {
                                    new PropertyMetadata
                                    {
                                        Path = "contacts.campaigns.response",
                                        Type = PropertyType.Object,
                                    },
                                },
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
                        Path = "sentiment",
                        Type = PropertyType.Object,
                        Children = new List<PropertyMetadata>
                        {
                            new PropertyMetadata
                            {
                                Path = "sentiment.csmPulse",
                                Type = PropertyType.Object,
                                Children = new List<PropertyMetadata>
                                {
                                    new PropertyMetadata
                                    {
                                        Path = "sentiment.csmPulse.latest",
                                        Type = PropertyType.Object
                                    }
                                }
                            }
                        }
                    },
                    new PropertyMetadata
                    {
                        Path = "support",
                        Type = PropertyType.Object,
                        Children = new List<PropertyMetadata>
                        {
                            new PropertyMetadata
                            {
                                Path = "support.tickets",
                                Type = PropertyType.Nested,
                            }
                        }
                    },
                    new PropertyMetadata
                    {
                        Path = "accounts",
                        Type = PropertyType.Object,
                        Children = new List<PropertyMetadata>
                        {
                            new PropertyMetadata
                            {
                                Path = "accounts.subscriptions",
                                Type = PropertyType.Nested,
                                Children = new List<PropertyMetadata>{
                                    new PropertyMetadata
                                    {
                                        Path = "accounts.subscriptions.customFields",
                                        Type = PropertyType.Object,
                                        Children = new List<PropertyMetadata>
                                        {
                                            new PropertyMetadata
                                            {
                                                Path = "accounts.subscriptions.customFields.texts",
                                                Type = PropertyType.Nested
                                            },
                                            new PropertyMetadata
                                            {
                                                Path = "accounts.subscriptions.customFields.numbers",
                                                Type = PropertyType.Nested,
                                            }
                                        },
                                    },
                                    new PropertyMetadata{
                                        Path = "accounts.subscriptions.summary",
                                        Type = PropertyType.Object
                                    },
                                    new PropertyMetadata
                                    {
                                        Path = "accounts.subscriptions.invoices",
                                        Type = PropertyType.Nested,
                                        Children = new List<PropertyMetadata>{
                                            new PropertyMetadata{
                                                Path = "accounts.subscriptions.invoices.summary",
                                                Type = PropertyType.Object,
                                            }
                                        }
                                    },
                                }
                            },
                        },
                    },
                    new PropertyMetadata
                    {
                        Path = "customFields",
                        Type = PropertyType.Object,
                        Children = new List<PropertyMetadata>
                        {
                            new PropertyMetadata
                            {
                                Path = "customFields.texts",
                                Type = PropertyType.Nested
                            },
                            new PropertyMetadata
                            {
                                Path = "customFields.numbers",
                                Type = PropertyType.Nested,
                            }
                        },
                    },
                };
        }
    }
}

