using INVApp.ViewModels;
using INVApp.Services;

namespace INVApp.Views;

public partial class CustomerPage : ContentPage
{

    public CustomerPage()
	{
		InitializeComponent();
        BindingContext = new CustomerPageViewModel(App.DatabaseService, App.APIService);
    }

}