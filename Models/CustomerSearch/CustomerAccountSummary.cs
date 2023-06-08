using System;
using System.Collections.Generic;

namespace QueryEditor.Models.Search
{
    public class CustomerAccountSummary
    {
        public decimal? Mrr { get; set; }

        public decimal? Arr { get; set; }

        public DateTime? NextRenewalDate { get; set; }

        public ICollection<SubscriptionSummary> Subscriptions { get; set; }
    }
}
