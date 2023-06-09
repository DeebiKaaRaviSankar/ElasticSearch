﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nest;
using QueryEditor.Models.Query;

namespace QueryEditor.Services.ElasticSearch 
{ 
    internal static class FilterConstructorService
    {
        internal static QueryContainer ConstructNotEqualToFilter(FilterDefinition filter)
        {
            return new BoolQuery
            {
                MustNot = new List<QueryContainer>
                        {
                            new QueryStringQuery
                            {
                                Fields = new Field(filter.Field),
                                Query = filter.Values.FirstOrDefault(),
                            },
                        },
            };
        }

        internal static QueryContainer ConstructLessThanOrEqualToXDayFromTodayFilter(FilterDefinition filter)
        {
            return new DateRangeQuery
            {
                Field = new Field(filter.Field),
                Format = "MM/dd/yyyy",
                LessThanOrEqualTo = "12/31/9999",
                GreaterThanOrEqualTo = DateTime.Today.AddDays(Convert.ToInt32(filter.Value)).ToString(@"MM\/dd\/yyyy"),
            };
            ////    new BoolQuery
            ////{
            ////    Must = new List<QueryContainer>
            ////            {
            ////                new DateRangeQuery
            ////                {
            ////                    Field = new Field(filter.Field),
            ////                    Format = "MM/dd/yyyy",
            ////                    LessThanOrEqualTo = "12/31/9999",
            ////                    GreaterThanOrEqualTo = DateTime.Today.AddDays(Convert.ToInt32(filter.Value)).ToString("MM/dd/yyyy"),
            ////                },
            ////            },
            ////};
        }

        internal static QueryContainer ConstructDateRangeFilter(FilterDefinition filter)
        {
            return new DateRangeQuery
            {
                Field = new Field(filter.Field),
                Format = "MM/dd/yyyy",
                GreaterThanOrEqualTo = filter.Values.ElementAt(0),
                LessThanOrEqualTo = filter.Values.ElementAt(1),
            };
        }

        internal static QueryContainer ConstructTermsFilter(string field, List<string> filterValues)
        {
            return new TermsQuery()
            {
                Field = new Field(field),
                Terms = filterValues,
            };
        }

        internal static QueryContainer ConstructSimpleQueryStringFilter(string field, string filterValue)
        {
            return new SimpleQueryStringQuery()
            {
                Fields = new Field(field),
                Query = filterValue,
                DefaultOperator = Operator.And,
            };
        }

        internal static QueryContainer ConstructQueryStringFilter(List<string> fields, string filterValue)
        {
            var queryStringQuery = new QueryStringQuery()
            {
                Query = "*" + filterValue + "*",
                DefaultOperator = Operator.And,
            };
            foreach (var field in fields)
            {
                if (queryStringQuery.Fields == null)
                {
                    queryStringQuery.Fields = new Field(field);
                    continue;
                }

                queryStringQuery.Fields.And(new Field(field));
            }

            return queryStringQuery;
        }

        internal static QueryContainer ConstructNestedQuery(string path, QueryContainer query, int from = 0, int pageSize = 0)
        {
            return new NestedQuery
            {
                Path = path,
                Query = query,
                InnerHits = new InnerHits
                {
                    From = from,
                    Size = pageSize,
                },
            };
        }
    }
}
