using INVApp.Models;
using INVApp.ViewModels;

namespace INVApp.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
        BindingContext = new LoginPageViewModel(App.DatabaseService);
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