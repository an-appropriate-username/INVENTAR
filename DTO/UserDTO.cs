using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INVApp.DTO
{
    public class UserDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Passcode { get; set; }
        public int Role { get; set; }
        public int ItemsScanned { get; set; }
        public int CustomersAdded { get; set; }
        public int TransactionsClosed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastLogin { get; set; }
    }
}
