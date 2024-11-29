using INVApp.ViewModels;
using INVApp.Services;

namespace INVApp.Views;

public partial class CustomerPage : ContentPage
{

    public CustomerPage()
	{
		InitializeComponent();

        var databaseService = new DatabaseService();
        var apiService = new APIService();
        var validationService = new ValidationService();

        BindingContext = new CustomerPageViewModel(databaseService, apiService, validationService);

        App.NotificationService.OnNotify += message => NotificationBanner.Show(message);
        App.NotificationService.OnConfirm += message => ConfirmBanner.Show(message);
    }

}