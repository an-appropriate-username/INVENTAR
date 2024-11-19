using INVApp.ViewModels;
using INVApp.Services;
using INVApp.Views;

namespace INVApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(POSPage), typeof(POSPage));
            Routing.RegisterRoute(nameof(StockOverviewPage), typeof(StockOverviewPage));
            Routing.RegisterRoute(nameof(StockIntakePage), typeof(StockIntakePage));
            Routing.RegisterRoute(nameof(TransactionLogPage), typeof(TransactionLogPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(CustomerPage), typeof(CustomerPage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));

        }
    }
}
