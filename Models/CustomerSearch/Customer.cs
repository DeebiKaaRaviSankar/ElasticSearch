using System;
using System.Collections.Generic;
using System.Text;

namespace QueryEditor.Models
{
    public class Customer
    {
        public Customer() { }

        public Customer(
            string name,
            int id = 0,
            int documentId = 0)
        {
            this.Name = name;
            this.Id = id;
            this.DocumentId = documentId;
        }

        public int Id { get; set; }
        public int DocumentId { get; set; }
        public string Name { get; set; }
        public string Domain { get; set; }
        public IEnumerable<Contact> Contacts { get; set; }
        public IEnumerable<Opportunities> Opportunities { get; set; }
    }
}
