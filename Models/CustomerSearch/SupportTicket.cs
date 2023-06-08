using System;

namespace QueryEditor.Models
{
    public class SupportTicket
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public string CreatedDate { get; set; }
    }
}
