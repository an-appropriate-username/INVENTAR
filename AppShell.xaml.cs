using INVApp.ViewModels;
using INVApp.Services;
using INVApp.Views;
using System.Windows.Input;

namespace INVApp
{
    public partial class AppShell : Shell
    {
        public ICommand LogOutCommand { get; }

        public AppShell()
        {
            InitializeComponent();
            LogOutCommand = new Command(async () =>
            {
                App.CurrentUser = null;

                var loginPage = new LoginPage();
                await Application.Current.MainPage.Navigation.PushModalAsync(loginPage);
            });

            BindingContext = this;

            Routing.RegisterRoute(nameof(POSPage), typeof(POSPage));
            Routing.RegisterRoute(nameof(StockOverviewPage), typeof(StockOverviewPage));
            Routing.RegisterRoute(nameof(StockIntakePage), typeof(StockIntakePage));
            Routing.RegisterRoute(nameof(TransactionLogPage), typeof(TransactionLogPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(CustomerPage), typeof(CustomerPage));
            Routing.RegisterRoute(nameof(CreateUserPage), typeof(CreateUserPage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
        }

        private async Task ShowLoginPageAsync()
        {
            await GoToAsync($"//{nameof(LoginPage)}");
        }

        public async Task NavigateToHomePageAsync()
        {
            await GoToAsync($"//{nameof(HomePage)}");
        }

    }
}
