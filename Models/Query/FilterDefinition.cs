using QueryEditor.Models.Query.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueryEditor.Models.Query
{
    public class FilterDefinition
    {
        private IEnumerable<string> values = new List<string>();

        public FilterDefinition()
        {
        }

        public FilterDefinition(string field, string @operator, object value)
        {
            this.Field = field;
            this.Operator = @operator;
            this.LogicalOperator = LogicalOperator.AND;
            this.Value = value;
        }

        public FilterDefinition(string field, string @operator, LogicalOperator logicalOperator, object value)
        {
            this.Field = field;
            this.Operator = @operator;
            this.LogicalOperator = logicalOperator;
            this.Value = value;
        }

        public FilterDefinition(List<string> fields, string @operator, LogicalOperator logicalOperator, object value)
        {
            this.Fields = fields;
            this.Operator = @operator;
            this.LogicalOperator = logicalOperator;
            this.Value = value;
        }

        public string Field { get; set; }

        public string Operator { get; set; }

        public LogicalOperator LogicalOperator { get; set; }

        public object Value { get; set; }

        public IEnumerable<string> Values
        {
            get
            {
                if (this.values != null && this.values.Any())
                {
                    return this.values;
                }

                return new List<string> { this.Value?.ToString() };
            }
            set => this.values = value;
        }

        public IEnumerable<string> Fields { get; set; } = new List<string>();

        public FilterTypes FilterType { get; set; }

        public bool FindExactMatches { get; set; }
    }
}
