using System;
using System.Collections.Generic;
using System.Text;

namespace QueryEditor.Models.Search
{
    public class CustomerContactCampaign
    {
        public CustomerContactCampaign()
        { }

       
        public string InstanceId { get; set; }

        public int Id { get; set; }

        public string FromEmail { get; set; }

        public DateTime TriggeredAt { get; set; }

        public DateTime RespondedOn { get; set; }
    }

}