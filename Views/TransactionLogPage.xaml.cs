using INVApp.ViewModels;

namespace INVApp.Views;

public partial class TransactionLogPage : ContentPage
{
	public TransactionLogPage()
	{
		InitializeComponent();
        BindingContext = new TransactionLogViewModel(App.DatabaseService, App.APIService);
    }

}