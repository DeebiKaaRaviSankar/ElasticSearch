using System;

namespace QueryEditor.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public double PlanAmount { get; set; }
        public int PlanQuantity { get; set; }
        public string PlanStartDate { get; set; }
        public string SubscriptionStartDate { get; set; }
        public string SubscriptionEndDate { get; set; }
        public string Status { get; set; }
        public string SubscriptionStatus { get; set; }
        public string ActionDate { get; set; }
        public double MonthlyRecurringRevenue { get; set; }
    }
}
