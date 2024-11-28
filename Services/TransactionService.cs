using INVApp.DTO;
using INVApp.Models;
using INVApp.NewFolder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INVApp.Services
{
    public class TransactionService
    {
        private readonly APIService _apiService;
        private readonly StockService _stockService;
        private readonly ReceiptService _receiptService;
        private readonly NotificationService _notificationService;

        public TransactionService(
            APIService apiService,
            StockService stockService,
            ReceiptService receiptService,
            NotificationService notificationService)
        {
            _apiService = apiService;
            _stockService = stockService;
            _receiptService = receiptService;
            _notificationService = notificationService;
        }

        public async Task<bool> ProcessCheckout(
            TransactionDto transactionDto,
            List<TransactionItemDto> transactionItems,
            Customer customer,
            string paymentMethod)
        {
            try
            {
                if (!ValidateCheckout(customer, paymentMethod))
                {
                    return false;
                }

                // Convert transaction items to CartItems for stock and receipt
                var cartItems = new ObservableCollection<CartItem>(
                    transactionItems.Select(item => new CartItem
                    {
                        Product = new Product { ProductID = item.ProductId },
                        Quantity = item.Quantity,
                        Price = item.UnitPrice,
                        ProductName = item.ProductName, // Make sure this property exists in TransactionItemDto
                        
                    }));

                await _stockService.DecrementStock(cartItems);

                // Pass all required parameters to GenerateReceipt
                var receipt = _receiptService.GenerateReceipt(
                    cartItems,
                    customer,
                    paymentMethod,
                    transactionDto.TotalAmount);

                return await _apiService.SaveTransactionAsync(transactionDto, transactionItems);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Checkout process error: {ex.Message}");
                return false;
            }
        }

        private bool ValidateCheckout(Customer customer, string paymentMethod)
        {
            if (string.IsNullOrEmpty(paymentMethod))
            {
                _notificationService.Notify("Please select a payment method.");
                return false;
            }

            if (customer == null || customer.Id <= 0)
            {
                _notificationService.Notify("Please select a valid customer.");
                return false;
            }

            return true;
        }
    }
}
