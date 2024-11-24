using INVApp.Services;
using INVApp.ViewModels;

namespace INVApp.Views;

public partial class CreateUserPage : ContentPage
{
	public CreateUserPage()
	{
		InitializeComponent();

        var databaseService = new DatabaseService();

        BindingContext = new CreateUserViewModel(databaseService);
    }
}