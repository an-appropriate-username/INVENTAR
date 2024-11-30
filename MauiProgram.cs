using Microsoft.Extensions.Logging;
using INVApp.Services;
using INVApp.ViewModels;
using INVApp.Views;
using INVApp.Interfaces;
using ZXing.Net.Maui.Controls;
using CommunityToolkit.Maui;
using Microsoft.Maui.LifecycleEvents;
using Syncfusion.Maui.Core.Hosting;
#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using WinRT.Interop; 
#endif

namespace INVApp
{
    public static class MauiProgram
    {
        /// <summary>
        /// Creates and configures a new instance of the MauiApp.
        /// </summary>
        /// <returns>The configured MauiApp instance.</returns>
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            #region Application Configuration

            builder
                .UseMauiApp<App>()
                .UseBarcodeReader()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMediaElement()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Lato-Regular.ttf", "LatoRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });



            #endregion

            #region Service Registrations

            // Register services
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<APIService>();
            builder.Services.AddSingleton<TransactionService>();
            builder.Services.AddSingleton<StockService>();
            builder.Services.AddSingleton<ReceiptService>();
            builder.Services.AddSingleton<SoundService>();
            builder.Services.AddSingleton<DatabaseConfigService>();
            builder.Services.AddSingleton<IValidationService, ValidationService>();


            // Register pages
            builder.Services.AddTransient<StockOverviewPage>();
            builder.Services.AddTransient<StockIntakePage>();
            builder.Services.AddTransient<POSPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<AuditTrailPage>();
            builder.Services.AddTransient<RestorePointPage>();
            builder.Services.AddTransient<TransactionLogPage>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<CustomerPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<AccountPage>();
            builder.Services.AddTransient<CreateUserPage>();
            builder.Services.AddTransient<CustomerDetailsPopup>();


            // Register view models
            builder.Services.AddTransient<StockOverviewViewModel>();
            builder.Services.AddTransient<StockIntakeViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<AuditTrailViewModel>();
            builder.Services.AddTransient<RestorePointViewModel>();
            builder.Services.AddTransient<TransactionLogViewModel>();
            builder.Services.AddTransient<HomePageViewModel>();
            builder.Services.AddTransient<CustomerPageViewModel>();
            builder.Services.AddTransient<AccountViewModel>();
            builder.Services.AddTransient<LoginPageViewModel>();
            builder.Services.AddTransient<CreateUserViewModel>();
            builder.Services.AddTransient<CustomerDetailsViewModel>();



            #endregion

            #region Logging Configuration

#if DEBUG
            builder.Logging.AddDebug();
#endif

            #endregion

            #region Window Configuration

            // Add lifecycle events and configure window behavior for Windows
            builder.ConfigureLifecycleEvents(events =>
            {
#if WINDOWS
                events.AddWindows(w =>
                {
                    w.OnWindowCreated(window =>
                    {
                        window.ExtendsContentIntoTitleBar = true;  // Ensure title bar is visible

                        IntPtr hWnd = WindowNative.GetWindowHandle(window);
                        WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
                        var _appWindow = AppWindow.GetFromWindowId(myWndId);

                        // Set the window to the default presenter and maximize it
                        _appWindow.SetPresenter(AppWindowPresenterKind.Default);  // Default mode keeps the title bar
                        _appWindow.Resize(new Windows.Graphics.SizeInt32(1920, 1080));  // Resize window to max size (or to screen size)
                        _appWindow.Move(new Windows.Graphics.PointInt32(0, 0));
                    });
                });
#endif
            });

            #endregion

            return builder.Build();
        }
    }
}
