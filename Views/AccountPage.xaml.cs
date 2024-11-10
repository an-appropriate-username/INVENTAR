using INVApp.ViewModels;

namespace INVApp.Views;

public partial class AccountPage : ContentPage
{
	public AccountPage()
	{
		InitializeComponent();
		BindingContext = new AccountViewModel(App.DatabaseService);
	}
}