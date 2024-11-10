using INVApp.Services;
using INVApp.ViewModels;

namespace INVApp.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();

        var databaseService = new DatabaseService();
        var databaseConfigService = new DatabaseConfigService(databaseService);

        BindingContext = new SettingsViewModel(databaseService, databaseConfigService);

        App.NotificationService.OnNotify += message => NotificationBanner.Show(message);
        App.NotificationService.OnConfirm += message => ConfirmBanner.Show(message);
    }
}