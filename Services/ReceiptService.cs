using INVApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INVApp.Services
{
    public class ReceiptService
    {
        public string GenerateReceipt(ObservableCollection<CartItem> cart, Customer customer, string paymentMethod, decimal totalAmount)
        {
            var receipt = new StringBuilder();
            receipt.AppendLine("Receipt");
            receipt.AppendLine("--------------------");

            foreach (var item in cart)
            {
                receipt.AppendLine($"{item.ProductName} x {item.Quantity} @ {item.Price:C} = {item.TotalPrice:C}");
            }

            receipt.AppendLine("--------------------");
            receipt.AppendLine($"Total: {totalAmount:C}");
            receipt.AppendLine($"Payment Method: {paymentMethod}");
            receipt.AppendLine($"Customer: {customer.CustomerName} (ID: {customer.Id})");
            receipt.AppendLine("Thank you for your purchase!");

            return receipt.ToString();
        }
    }
}
