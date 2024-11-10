using INVApp.ViewModels;
using INVApp.Models;

namespace INVApp.Views;

public partial class ProductDetailsPopup : ContentPage
{
    private readonly StockOverviewViewModel _overviewViewModel;
    private readonly ProductDetailViewModel _viewModel;

    public ProductDetailsPopup(Product product, StockOverviewViewModel overviewViewModel)
	{
		InitializeComponent();

        _overviewViewModel = overviewViewModel;

        _viewModel = new ProductDetailViewModel(App.DatabaseService)
        {
            Product = product 
        };

        _viewModel.ProductUpdated += OnProductUpdated;
        _viewModel.ProductDeleted += OnProductDeleted;

        BindingContext = _viewModel;
    }

    private void OnProductUpdated()
    {
        // Find the product in the collection and update its details
        var existingProduct = _overviewViewModel.Products.FirstOrDefault(p => p.ProductID == _viewModel.Product.ProductID);

        if (existingProduct != null)
        {
            // Update the existing product with new values
            existingProduct.ProductName = _viewModel.Product.ProductName;
            existingProduct.CurrentStockLevel = _viewModel.Product.CurrentStockLevel;
            existingProduct.Category = _viewModel.Product.Category;

            // Notify the UI that the product has changed
            _overviewViewModel.Products[_overviewViewModel.Products.IndexOf(existingProduct)] = existingProduct;
        }
    }

    private void OnProductDeleted()
    {
        // Find and remove the deleted product from the collection
        var existingProduct = _overviewViewModel.Products.FirstOrDefault(p => p.ProductID == _viewModel.Product.ProductID);

        if (existingProduct != null)
        {
            _overviewViewModel.Products.Remove(existingProduct); // Remove from the collection
        }
    }

}