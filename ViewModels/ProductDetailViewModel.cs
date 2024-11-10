using System;
using System.Windows.Input;
using INVApp.Models;
using INVApp.Services;
using INVApp.Views;

namespace INVApp.ViewModels
{
    /// <summary>
    /// ViewModel for showing an overview of a selected product. Allows updating and deleting. 
    /// </summary>
    public class ProductDetailViewModel : BaseViewModel
    {
        #region Declarations

        // Declare database service
        private readonly DatabaseService _databaseService;

        // Declare events for product updates and deletions
        public event Action? ProductUpdated;
        public event Action? ProductDeleted;

        // Declare the Product object
        private Product? _product;
        public Product? Product
        {
            get => _product;
            set
            {
                _product = value;
                OnPropertyChanged(nameof(Product));
            }
        }

        // Declare Commands for product actions
        public ICommand CloseCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }

        #endregion

        #region Constructor

        public ProductDetailViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            CloseCommand = new Command(CloseModal);
            UpdateCommand = new Command(UpdateProduct);
            DeleteCommand = new Command(DeleteProduct);
        }

        #endregion

        #region Command Methods

        private async void CloseModal()
        {
            await Application.Current.MainPage.Navigation.PopModalAsync();
        }

        /// <summary>
        /// Updates the product details in the database.
        /// </summary>
        private async void UpdateProduct()
        {
            // Validate stock level
            if (Product.CurrentStockLevel < 0)
            {
                await Application.Current.MainPage.DisplayAlert("Invalid Stock Level", "Stock level cannot be negative. Please correct the stock level before updating.", "OK");
                return;
            }

            // Confirm update
            Application.Current.Dispatcher.Dispatch(async () =>
            {
                bool answer = await Application.Current.MainPage.DisplayAlert("Update Product", "Are you sure you want to update the product details?", "Yes", "No");
                if (answer)
                {
                    var initialProductDetails = await _databaseService.GetProductByIDAsync(Product.ProductID);
                    var initialCurrentStock = initialProductDetails.CurrentStockLevel;
                    var stockAdjustment = Product.CurrentStockLevel - initialCurrentStock;

                    await _databaseService.UpdateProductAsync(Product, stockAdjustment);
                    ProductUpdated?.Invoke();

                    await Application.Current.MainPage.Navigation.PopModalAsync();
                }
            });
        }

        /// <summary>
        /// Deletes the product from the database.
        /// </summary>
        private async void DeleteProduct()
        {
            // Confirm deletion
            bool answer = await Application.Current.MainPage.DisplayAlert("Delete Product", "Are you sure you want to delete this product?", "Yes", "No");
            if (answer)
            {
                App.NotificationService.Notify($"Deleting product: {Product.ProductName}");
                await _databaseService.DeleteProductAsync(Product);

                ProductDeleted?.Invoke();

                await Application.Current.MainPage.Navigation.PopModalAsync();
            }
        }

        #endregion
    }
}
