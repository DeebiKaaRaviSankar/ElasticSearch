using Nest;
using QueryEditor.Models;
using QueryEditor.Models.Query;
using QueryEditor.Models.Query.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryEditor.Services
{
    public static class SearchService
    {
        public static UserInputDataType GetUserInputDataType(string userInputValue)
        {

            bool boolValue;
            int intValue;
            long bigintValue;
            double doubleValue;
            DateTime dateValue;

            // Place checks higher in if-else statement to give higher priority to type.

            if (bool.TryParse(userInputValue, out boolValue))
                return UserInputDataType.Boolean;
            else if (Int32.TryParse(userInputValue, out intValue))
                return UserInputDataType.Int;
            else if (Int64.TryParse(userInputValue, out bigintValue))
                return UserInputDataType.Long;
            else if (double.TryParse(userInputValue, out doubleValue))
                return UserInputDataType.Double;
            else if (DateTime.TryParse(userInputValue, out dateValue))
                return UserInputDataType.DateTime;
            else return UserInputDataType.String;

        }


        public static QueryContainer GetQueryRequestFromUserInput()
        {
            QueryContainer result = new QueryContainer();

            //string[] emptySpaceSplitString = userInput.Split(" ");

            //string[] dotSplitString = userInput.Split(".");

            //string indexName = dotSplitString[0];
            //string fieldName = dotSplitString[1];

            QueryContainer filters = null;


            var now = DateTime.Now;
            var timeSpan = new TimeSpan(35);

            var minDate = now.Subtract(timeSpan);
            var maxDate = now;

            var maxUsers = new QueryContainerDescriptor<Customer>().Bool(b => b.Should(sh => sh.Match(mp => mp.Field("customer.licensing.maxUsers").Query("20"))));

            var maxUsers1 = new QueryContainerDescriptor<Customer>().Term(term=>term.Field("customer.licensing.maxUsers").Value("20"));

            filters &= maxUsers1;


            return result;
        }
    }
}
