using ZXing.Net.Maui;
using INVApp.Services;
using INVApp.ViewModels;

namespace INVApp.Views;

public partial class POSPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly APIService _apiSrervice;

    private DateTime _lastScanTime;
    private readonly TimeSpan _scanInterval = TimeSpan.FromMilliseconds(1000);

    public POSPage()
	{
		InitializeComponent();

        _databaseService = new DatabaseService();
        _apiSrervice = new APIService();

        BindingContext = new POSViewModel(_databaseService, _apiSrervice);

        App.NotificationService.OnNotify += message => NotificationBanner.Show(message);
        App.NotificationService.OnConfirm += message => ConfirmBanner.Show(message);

        /*
        cameraBarcodeReaderView.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormats.OneDimensional,
            AutoRotate = true,
            Multiple = true
        };
        */

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