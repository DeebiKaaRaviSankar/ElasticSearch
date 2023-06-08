using System;
using System.Collections.Generic;

namespace QueryEditor.Models.Search
{
    public class CustomerJourneySummary
    {
        public int StageId { get; set; }

        public DateTime StartDayInStage { get; set; }
    }
}
