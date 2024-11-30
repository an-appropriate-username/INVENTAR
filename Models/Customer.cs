using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace INVApp.Models
{ 
    public class Customer
    {
        internal string? FirstName;
        internal string? LastName;
        internal string? PhoneNumber;
        internal bool IsLoyaltyMember;

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public string? Barcode { get; set; }
        public string? CustomerName { get; set; }
        public string? Surname { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public bool IsMember { get; set; }

    }
}
