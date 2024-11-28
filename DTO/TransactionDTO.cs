using INVApp.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INVApp.NewFolder
{
    public class TransactionDto
    {
        public DateTime TransactionDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public string PaymentMethod { get; set; }
        public decimal TaxAmount { get; set; }
        public int UserId { get; set; }
        public int CustomerId { get; set; }
        public CustomerDto Customer { get; set; }  // Added this property
        public List<TransactionItemDto> TransactionItems { get; set; } = new List<TransactionItemDto>();
    }
}
