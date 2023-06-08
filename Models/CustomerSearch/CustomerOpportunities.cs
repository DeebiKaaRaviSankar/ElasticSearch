using System.Collections.Generic;

namespace QueryEditor.Models.Search
{
    public class CustomerOpportunities
    {
        public CustomerOpportunities()
        {
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public int ContactId { get; set; }

        public IEnumerable<string> CustomTags { get; set; }

#nullable enable
        public CustomerSearch? Customer { get; set; }
    }
}
