using ElasticSearchPatchGenaration.Models;
using System;
using System.Collections.Generic;

namespace QueryEditor.Models.Search
{
    public class CustomerSearch : Editable
    {
        public CustomerSearch()
        {
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string CustomerSource { get; set; }

        public string ExternalId { get; set; }

        public string CustomerSuccessManagerId { get; set; }

        public int? IndustryId { get; set; }

        public int? CountryId { get; set; }

        public int? TimeZoneId { get; set; }

        public string Domain { get; set; }

        public string CrmDomain { get; set; }

        public int Status { get; set; }

        public bool? IsNew { get; set; }

        public string Identifier { get; set; }

        public string CustomerAge { get; set; }

        public CustomerAccountSummary Account { get; set; }

        public IEnumerable<string> CustomTags { get; set; }

        public IEnumerable<CustomerContact> Contacts { get; set; }

        public IEnumerable<CustomerOpportunities> Opportunities { get; set; }

        public SupportTicketSummary SupportTickets { get; set; }

        public TaskSummary Tasks { get; set; }

        public CustomerSentimentSummary Sentiment { get; set; }

        public UsageSummary Usage { get; set; }

        public ICollection<int> Segments { get; set; }

        public CustomerJourneySummary CustomerJourney { get; set; }

        public HealthScoreSummary HealthScore { get; set; }

        public class PropertyData
        {
            public string Name;
            public PropertyType PropertyType;
            public string Path;

            public PropertyData(
                string name,
                PropertyType type,
                string path)
            {
                this.Name = name;
                this.PropertyType = type;
                this.Path = path;
            }
        }
    }
}
