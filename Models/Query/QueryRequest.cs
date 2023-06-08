using System;
using System.Collections.Generic;

namespace QueryEditor.Models.Query
{
    public class QueryRequest
    {
        public QueryRequest() { }

        public QueryRequest(Guid id, IEnumerable<FilterGroup> FilterGroups) {
            this.Id = id;
            this.FilterGroups = FilterGroups;
        }

        public Guid Id { get; set; }

        public IEnumerable<FilterGroup> FilterGroups { get; set; }
    }
}
