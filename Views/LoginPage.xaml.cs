using INVApp.ViewModels;

namespace INVApp.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
        BindingContext = new LoginPageViewModel(App.DatabaseService);
    }

}