using System;
using System.Collections.Generic;
using System.Text;

namespace QueryEditor.Models
{
    public class Opportunities
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int CustomerId { get; set; }
        public int ContactId { get; set; }
    }
}
