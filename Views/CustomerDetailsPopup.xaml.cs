using INVApp.ViewModels;
using INVApp.Models;

namespace INVApp.Views;

public partial class CustomerDetailsPopup : ContentPage
{
	public CustomerDetailsPopup(CustomerDetailsViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;

	}
}