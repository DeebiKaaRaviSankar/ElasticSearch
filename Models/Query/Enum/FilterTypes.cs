using System;
using System.Collections.Generic;
using System.Text;

namespace QueryEditor.Models.Query.Enum
{
    public enum FilterTypes
    {
        /// <summary>
        /// Represents a text based filter.
        /// </summary>
        Text = 1,

        /// <summary>
        /// Represents a date range filter.
        /// </summary>
        DateRange = 2,

        /// <summary>
        /// Represent a not equal to filter.
        /// </summary>
        NotEqualTo = 3,

        /// <summary>
        /// Represents a contains filter.
        /// </summary>
        Contains = 4,

        /// <summary>
        /// Represent a not equal to filter.
        /// </summary>
        LessThanOrEqualToXDayFromToday = 5,
    }
}
