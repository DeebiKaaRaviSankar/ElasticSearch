using QueryEditor.Models.Query.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryEditor.Models.Query
{
    public class FilterGroup
    {
        public FilterGroup() { }

        public FilterGroup(LogicalOperator logicalOperator, IEnumerable<FilterDefinition> filters)
        {
            this.LogicalOperator = logicalOperator;
            this.Filters = filters;
        }

        public LogicalOperator LogicalOperator { get; set; }
        public IEnumerable<FilterDefinition> Filters { get; set; }
    }
}
