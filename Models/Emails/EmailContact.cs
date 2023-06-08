using System;
using System.Collections.Generic;
using System.Text;

namespace QueryEditor.Models
{
    public class EmailContact
    {
        public EmailContact()
        {
        }

        public EmailContact(string name, string address)
        {
            this.Name = name;
            this.Address = address;
        }

        public string Name { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Address { get; set; }
    }
}
