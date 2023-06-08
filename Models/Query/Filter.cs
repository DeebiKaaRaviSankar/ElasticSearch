using QueryEditor.Models.Query;
using QueryEditor.Models.Query.Enum;

namespace QueryEditor.Models
{
    public class Filter
    {
        public Filter() { }

        public Filter(FilterCondition condition, TestFilterDefinition filterDefinition) {
            this.Condition = condition;
            this.FilterDefinition = filterDefinition;
        }

        public FilterCondition Condition { get; set; }
        public TestFilterDefinition FilterDefinition { get; set; }
    }
}
