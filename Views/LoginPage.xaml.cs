using INVApp.Models;
using INVApp.ViewModels;
using INVApp.Services;

namespace INVApp.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
        var databaseService = new DatabaseService();
        var apiService = new APIService();

        BindingContext = new LoginPageViewModel(databaseService, apiService);

        App.NotificationService.OnNotify += message => NotificationBanner.Show(message);
        App.NotificationService.OnConfirm += message => ConfirmBanner.Show(message);
    }

    private void OnSelectButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            var selectedUser = button.BindingContext as User;

            if (selectedUser != null)
            {
                UserIdEntry.Text = selectedUser.UserId.ToString();
                PasscodeEntry.Focus();
            }
        }
    }

}