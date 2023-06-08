using System.Collections.Generic;

namespace QueryEditor.Models.Query
{
    public class SearchRequest
    {
        public SearchRequest()
        {
            this.Fields = new List<string>();
            this.FilterDefinitions = new List<FilterDefinition>();
            this.PageNumber = 1;
            this.PageSize = 10;
        }

        public List<string> Fields { get; set; }

        public List<FilterDefinition> FilterDefinitions { get; set; }

        public List<FilterGroup> FilterGroups { get; set; }

        public int PageSize { get; set; }

        public int PageNumber { get; set; }

        public string Query { get; set; }

        public bool DoNotPaginate { get; set; }

        public List<string> FieldsToReturn { get; set; }
    }
}
