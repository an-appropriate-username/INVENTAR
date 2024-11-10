using ZXing.Net.Maui;
using INVApp.Services;
using INVApp.ViewModels;

namespace INVApp.Views;

public partial class POSPage : ContentPage
{
    private readonly DatabaseService _databaseService;

    private DateTime _lastScanTime;
    private readonly TimeSpan _scanInterval = TimeSpan.FromMilliseconds(1000);

    public POSPage()
	{
		InitializeComponent();

        _databaseService = new DatabaseService();

        BindingContext = new POSViewModel(_databaseService);

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
        var viewModel = BindingContext as POSViewModel;
        if (viewModel != null && e.Results.Any())
        {

            var now = DateTime.Now;
            if (now - _lastScanTime < _scanInterval)
            {
                return;
            }

            _lastScanTime = now;

            var barcode = e.Results.First();
            await viewModel.ProcessScannedBarcode(barcode.Value);
        }
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await Task.Delay(100);
        BarcodeEntry.Focus();
    }
}