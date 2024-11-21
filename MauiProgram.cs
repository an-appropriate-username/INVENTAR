using Microsoft.Extensions.Logging;
using INVApp.Services;
using INVApp.ViewModels;
using INVApp.Views;
using ZXing.Net.Maui.Controls;
using CommunityToolkit.Maui;

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
            builder.Services.AddSingleton<SoundService>();
            builder.Services.AddSingleton<DatabaseConfigService>();


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


            #endregion

            #region Logging Configuration

#if DEBUG
            builder.Logging.AddDebug();
#endif

            #endregion

            return builder.Build();
        }
    }
}
