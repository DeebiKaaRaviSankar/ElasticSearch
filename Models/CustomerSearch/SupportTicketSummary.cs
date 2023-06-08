using System;
using System.Collections.Generic;

namespace QueryEditor.Models.Search 
{ 
    public class SupportTicketSummary
    {
        public int Total { get; set; }

        public int Open { get; set; }

        public int InProgress { get; set; }

        public int Closed { get; set; }

        public int Overdue { get; set; }
    }
}
