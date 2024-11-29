using INVApp.Services;
using INVApp.ViewModels;

namespace INVApp.Views;

public partial class CreateUserPage : ContentPage
{
	public CreateUserPage()
	{
		InitializeComponent();

        var databaseService = new DatabaseService();
        var validationService = new ValidationService();

        BindingContext = new CreateUserViewModel(databaseService, validationService);

        App.NotificationService.OnNotify += message => NotificationBanner.Show(message);
        App.NotificationService.OnConfirm += message => ConfirmBanner.Show(message);
    }
}