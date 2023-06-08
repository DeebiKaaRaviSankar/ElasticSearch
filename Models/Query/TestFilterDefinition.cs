using QueryEditor.Models.Query.Enum;
using QueryEditor.Services;

namespace QueryEditor.Models.Query
{
    public class TestFilterDefinition
    {
        public TestFilterDefinition() { }

        public TestFilterDefinition(string fieldName, FilterOperator @operator, string userInputValue, bool isExpression = false) {
            this.FieldName = fieldName;
            this.IsExpression = isExpression;
            this.Operator = @operator;
            this.UserInputValue = userInputValue;
            this.UserInputDataType = SearchService.GetUserInputDataType(userInputValue: userInputValue);
        }

        public string FieldName { get; set; }

        public bool IsExpression { get; set; }

        public FilterOperator Operator { get; set; }

        public string UserInputValue { get; set; }

        public UserInputDataType UserInputDataType { get; set; }
    }
}
