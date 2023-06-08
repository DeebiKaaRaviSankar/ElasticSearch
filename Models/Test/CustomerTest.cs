using QueryEditor.Models.Test;
using System.Collections.Generic;

namespace QueryEditor.Models
{
    public class CustomerTest
    {
        public CustomerTest() { }

        public CustomerTest(int id, string domain, int status, string name, List<ContactTest> contacts) {
            this.Id = id;
            this.Domain = domain;
            this.Status = status;
            this.Name = name;
            this.Contacts = contacts;
        }

        public int Id { get; set; }
        public string Domain { get; set; }
        public int Status { get; set; }
        public string Name { get; set; }
        public List<ContactTest> Contacts { get; set; }
    }
}
