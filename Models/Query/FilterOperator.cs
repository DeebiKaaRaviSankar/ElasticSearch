using System;
using System.Collections.Generic;
using System.Text;

namespace QueryEditor.Models.Query
{
    public class FilterOperator
    {
        public FilterOperator(string name, string displayName, string symbol, string value = "")
        {
            this.Name = name;
            this.DisplayName = displayName;
            this.Symbol = symbol;
            this.Value = value;
        }

        public string Name;

        public string DisplayName;

        public string Value;

        public string Symbol;

        public static readonly FilterOperator OperatorNameGreaterThan = new FilterOperator(
                name: "GreaterThan",
                displayName: "Greater than",
                symbol: ">=",
                value: "gt"
           );
        public static readonly FilterOperator OperatorNameLessThan = new FilterOperator(
                name: "LessThan",
                displayName: "Less than",
                symbol: "<",
                value: "lt"
           );
        public static readonly FilterOperator OperatorNameGreaterThanOrEqualsTo = new FilterOperator(
                name: "GreaterThanOrEqualsTo",
                displayName: "Greater than or equals to",
                symbol: ">=",
                value: "gte"
           );
        public static readonly FilterOperator OperatorNameLessThanOrEqualsTo = new FilterOperator(
                name: "LessThanOrEqualsTo",
                displayName: "Less than or equals to",
                symbol: "<=",
                value: "lte"
           );
        public static readonly FilterOperator OperatorNameEqualTo = new FilterOperator(
                name: "EqualTo",
                displayName: "Equal to",
                symbol: "="
           );
        public static readonly FilterOperator OperatorNameNotEqualTo = new FilterOperator(
                name: "NotEqualTo",
                displayName: "Not equal to",
                symbol: "!="
           );

        public static readonly IEnumerable<FilterOperator> SupportedOperators = new List<FilterOperator>
        {
            FilterOperator.OperatorNameGreaterThan,
            FilterOperator.OperatorNameLessThan,
            FilterOperator.OperatorNameGreaterThanOrEqualsTo,
            FilterOperator.OperatorNameLessThanOrEqualsTo,
            FilterOperator.OperatorNameEqualTo,
            FilterOperator.OperatorNameEqualTo
        };


    }
}
