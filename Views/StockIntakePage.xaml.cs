using INVApp.ViewModels;
using INVApp.Services;
using ZXing.Net.Maui;
using INVApp.ContentViews;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Platform;

namespace INVApp.Views;

public partial class StockIntakePage : ContentPage
{

    // Declare variables

    private readonly DatabaseService _databaseService;
    private readonly APIService _apiService;
    private readonly SoundService _soundService;

    public double SoundVolume { get; private set; }
    public bool IsSoundEnabled { get; private set; }

    private DateTime _lastScanTime;
    private readonly TimeSpan _scanInterval = TimeSpan.FromMilliseconds(1000);

    public StockIntakePage()
    {
        InitializeComponent();

        _databaseService = new DatabaseService();
        _apiService = new APIService();
        _soundService = new SoundService();

        BindingContext = new StockIntakeViewModel(_databaseService, _apiService);

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            _soundService.Initialize(AudioPlayer);
            await LoadAudioSettings();
        });

        App.NotificationService.OnNotify += message => NotificationBanner.Show(message);

        cameraBarcodeReaderView.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormats.OneDimensional,
            AutoRotate = true,
            Multiple = true
        };

        _lastScanTime = DateTime.Now;
    }

    // Barcode Detected
    private async void BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        var viewModel = BindingContext as StockIntakeViewModel;
        if (viewModel != null && e.Results.Any())
        {

            var now = DateTime.Now;
            if (now - _lastScanTime < _scanInterval)
            {
                return;
            }

            await LoadAudioSettings();

            if (IsSoundEnabled)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await _soundService.PlaySoundAsync("scan_beep.mp3");
                });
            }

            _lastScanTime = now;

            var barcode = e.Results.First();
            await viewModel.ProcessScannedBarcode(barcode.Value);
        }
    }

    // Load audio settings
    private async Task LoadAudioSettings()
    {
        var settings = await _databaseService.GetAudioSettingsAsync();
        SoundVolume = settings.Volume;
        IsSoundEnabled = settings.IsEnabled;

        _soundService.ApplyVolume(SoundVolume);
    }

    // On appearing
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var viewModel = BindingContext as StockIntakeViewModel;
        if (viewModel != null)
        {
            await viewModel.LoadCategories();
        }

        //await DisplayAlert("Debug", "OnAppearing triggered", "OK");
    }

}