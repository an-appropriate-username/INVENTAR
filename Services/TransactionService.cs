using INVApp.DTO;
using INVApp.Models;
using INVApp.NewFolder;
using INVApp.Services;
using System.Diagnostics;
using System.Net.Http.Json;

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
            // First, create the transaction
            var response = await _apiService.SaveTransactionAsync(transactionDto);
            if (!response.IsSuccessStatusCode)
                return false;

            // Get the transaction ID from the response
            var createdTransaction = await response.Content.ReadFromJsonAsync<TransactionResponse>();
            if (createdTransaction?.TransactionId == null)
                return false;

            // Send the transaction items
            var itemsResponse = await _apiService.SaveTransactionItemsAsync(
                createdTransaction.TransactionId,
                transactionItems);

            return itemsResponse.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ProcessCheckout error: {ex.Message}");
            return false;
        }
    }
}

public class TransactionResponse
{
    public int TransactionId { get; set; }
}