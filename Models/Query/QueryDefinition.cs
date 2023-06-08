using QueryEditor.Models.Query.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryEditor.Models.Query
{
    class QueryDefinition
    {
        public QueryDefinition()
        {
        }

        public QueryDefinition(QueryTypes type, IEnumerable<string> fields, string path = "")
        {
            this.Type = type;
            this.Fields = fields;
            this.Path = path;
        }

        public QueryTypes Type { get; set; }

        public string Path { get; set; } = string.Empty;

        public IEnumerable<string> Fields { get; set; } = new List<string>();
    }
}
