using INVApp.ViewModels;
using INVApp.Services;

namespace INVApp.Views;

public partial class RestorePointPage : ContentPage
{
    private readonly RestorePointViewModel _viewModel;
    public RestorePointPage()
    {
        InitializeComponent();

        var databaseConfigService = App.DatabaseConfigService;

        if (databaseConfigService == null)
        {
            // Handle the null case (e.g., display an error message or throw an exception)
            throw new InvalidOperationException("DatabaseConfigService is not initialized.");
        }

        // Initialize the ViewModel and set it as the BindingContext
        _viewModel = new RestorePointViewModel(databaseConfigService);
        BindingContext = _viewModel;
    }
}