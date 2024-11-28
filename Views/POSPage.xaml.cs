using ZXing.Net.Maui;
using INVApp.Services;
using INVApp.ViewModels;

namespace INVApp.Views;

public partial class POSPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly APIService _apiService;

    private DateTime _lastScanTime;
    private readonly TimeSpan _scanInterval = TimeSpan.FromMilliseconds(1000);

    public POSPage()
    {
        InitializeComponent();

        // Create service instances
        _databaseService = new DatabaseService();
        _apiService = new APIService();

        // Create the additional required services
        var stockService = new StockService(_databaseService);
        var receiptService = new ReceiptService();
        var transactionService = new TransactionService(
            _apiService,
            stockService,
            receiptService,
            App.NotificationService);

        // Create view model with all required services
        BindingContext = new POSViewModel(
            _databaseService,
            _apiService,
            transactionService,
            stockService,
            receiptService);

        // Set up notifications
        App.NotificationService.OnNotify += message => NotificationBanner.Show(message);
        App.NotificationService.OnConfirm += message => ConfirmBanner.Show(message);

        _lastScanTime = DateTime.Now;
    }
    private async void BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (e.Results.Any() && DateTime.UtcNow - _lastScanTime >= _scanInterval)
        {
            _lastScanTime = DateTime.UtcNow;

            var barcode = e.Results.FirstOrDefault();
            if (barcode != null && BindingContext is POSViewModel viewModel)
            {
                await viewModel.ProcessScannedBarcode(barcode.Value);
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await Task.Delay(100);
        BarcodeEntry.Focus();
    }
}