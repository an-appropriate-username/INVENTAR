using INVApp.ViewModels;
using INVApp.Services;

namespace INVApp.Views;

public partial class StockOverviewPage : ContentPage
{
	public StockOverviewPage(StockOverviewViewModel viewModel)
	{
		InitializeComponent();

        BindingContext = viewModel;
    }

}