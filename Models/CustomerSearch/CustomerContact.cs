namespace QueryEditor.Models.Search
{
    public class CustomerContact
    {
        public CustomerContact()
        {
        }

        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public int CustomerId { get; set; }

        public CustomerContactCampaign Campaigns {get;set;}

#nullable enable
        public CustomerSearch? Customer { get; set; }
    }
}
