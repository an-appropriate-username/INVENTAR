using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using INVApp.Models;
using INVApp.Services;

namespace INVApp.ViewModels
{
    public class StockIntakeViewModel : BaseViewModel
    {
        #region Fields

        private readonly DatabaseService _databaseService;
        private string _selectedCategory;
        private string _scannedBarcode;
        private string _initialProductName;
        private string _initialBrandName;
        private string _initialCategory;
        private string _initialProductWeight;
        private decimal _initialWholesalePrice;
        private decimal _initialPrice;
        private int _stockAdjustment;
        private bool _isStockReduction;
        private string _stockReductionReason;
        public bool _isCameraOn;

        #endregion

        #region Constructor

        public StockIntakeViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            // Initialize Commands
            AddStockCommand = new Command(async () => await AddStock());
            ResetCommand = new Command(ResetFields);
            ToggleCameraCommand = new Command(ToggleCamera);

            IsStockReduction = false;

            Categories = new ObservableCollection<string>();

            LoadCategories();
            LoadDefaultCategory();
        }

        #endregion

        #region Properties

        public Product? SelectedProduct { get; set; }
        public string ProductName { get; set; }
        public string BrandName { get; set; }
        public string ProductWeight { get; set; }
        public int CurrentStockLevel { get; set; }
        public decimal WholesalePrice { get; set; }
        public decimal Price { get; set; }
        public ObservableCollection<string> Categories { get; set; }

        public string ScannedBarcode
        {
            get => _scannedBarcode;
            set
            {
                _scannedBarcode = value;
                OnPropertyChanged();
            }
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(IsCategoryChanged));
                OnPropertyChanged(nameof(SelectedCategory));
            }
        }

        public int StockAdjustment
        {
            get => _stockAdjustment;
            set
            {
                _stockAdjustment = value;
                OnPropertyChanged();
                IsStockReduction = _stockAdjustment < 0;
                OnPropertyChanged(nameof(IsStockReduction));
            }
        }

        public bool IsStockReduction
        {
            get => _isStockReduction;
            set
            {
                _isStockReduction = value;
                OnPropertyChanged();
            }
        }

        public string StockReductionReason
        {
            get => _stockReductionReason;
            set
            {
                _stockReductionReason = value;
                OnPropertyChanged();
            }
        }

        public bool IsCameraOn => _isCameraOn;

        #endregion

        #region Property Change Flags

        public bool IsCategoryChanged => SelectedCategory != _initialCategory;
        public bool IsWeightChanged => ProductWeight != _initialProductWeight;
        public bool IsNameChanged => ProductName != _initialProductName;
        public bool IsWholesalePriceChanged => WholesalePrice != _initialWholesalePrice;
        public bool IsSalePriceChanged => Price != _initialPrice;

        #endregion

        #region Commands

        public ICommand AddStockCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand ToggleCameraCommand { get; }

        #endregion

        #region Methods

        public async Task ProcessScannedBarcode(string barcode)
        {
            var product = await _databaseService.GetProductByBarcodeAsync(barcode);
            ToggleCamera();

            if (product != null)
            {
                // Populate fields with product data
                SelectedProduct = product;
                ScannedBarcode = barcode;
                ProductName = product.ProductName;
                BrandName = product.BrandName;
                SelectedCategory = product.Category;
                ProductWeight = product.ProductWeight;
                CurrentStockLevel = product.CurrentStockLevel;
                WholesalePrice = product.WholesalePrice;
                Price = product.Price;

                // Store initial values for change tracking
                _initialProductName = product.ProductName;
                _initialBrandName = product.BrandName;
                _initialCategory = product.Category;
                _initialProductWeight = product.ProductWeight;
                _initialWholesalePrice = product.WholesalePrice;
                _initialPrice = product.Price;

                // Notify user
                Application.Current.Dispatcher.Dispatch(() =>
                {
                    App.NotificationService.Notify($"Match found: {product.ProductName}.");
                });
            }
            else
            {
                ResetFields();
                ScannedBarcode = barcode;
                LoadDefaultCategory();

                Application.Current.Dispatcher.Dispatch(() =>
                {
                    App.NotificationService.Notify("No match found. Create new entry.");
                });
            }

            OnPropertyChanged(nameof(ScannedBarcode));
            OnPropertyChanged(nameof(ProductName));
            OnPropertyChanged(nameof(BrandName));
            OnPropertyChanged(nameof(SelectedCategory));
            OnPropertyChanged(nameof(ProductWeight));
            OnPropertyChanged(nameof(CurrentStockLevel));
            OnPropertyChanged(nameof(WholesalePrice));
            OnPropertyChanged(nameof(Price));
        }

        public async Task LoadCategories()
        {
            var categories = await _databaseService.GetCategoriesAsync();
            Application.Current.Dispatcher.Dispatch(() =>
            {
                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category.CategoryName);
                }
            });
        }

        private async void LoadDefaultCategory()
        {
            var defaultCategory = await _databaseService.GetDefaultCategoryAsync();
            if (!string.IsNullOrEmpty(defaultCategory))
            {
                SelectedCategory = defaultCategory;
            }
        }

        public async Task AddStock()
        {
            #region Data Validation

            if (string.IsNullOrEmpty(ScannedBarcode) || string.IsNullOrEmpty(ProductName) || string.IsNullOrEmpty(SelectedCategory) || string.IsNullOrEmpty(ProductWeight))
            {
                App.NotificationService.Notify("Please fill in all fields before updating inventory.");
                return;
            }

            if (StockAdjustment < 0 && string.IsNullOrEmpty(StockReductionReason))
            {
                App.NotificationService.Notify("Please provide a reason for stock reduction.");
                return;
            }

            #endregion

            if (SelectedProduct != null)
            {
                #region Update Existing Product

                if (SelectedProduct.CurrentStockLevel + StockAdjustment < 0)
                {
                    App.NotificationService.Notify("Stock level cannot go below zero.");
                    return;
                }

                if (IsCategoryChanged || IsWeightChanged || IsNameChanged || IsWholesalePriceChanged || IsSalePriceChanged)
                {
                    bool confirm = await Application.Current.MainPage.DisplayAlert(
                        "Confirm Changes",
                        "You have modified some product details. Are you sure you want to update?",
                        "Yes", "No");

                    if (!confirm) return;
                }

                SelectedProduct.ProductName = ProductName;
                SelectedProduct.BrandName = BrandName;
                SelectedProduct.Category = SelectedCategory;
                SelectedProduct.ProductWeight = ProductWeight;
                SelectedProduct.CurrentStockLevel += StockAdjustment;
                SelectedProduct.WholesalePrice = WholesalePrice;
                SelectedProduct.Price = Price;

                await _databaseService.UpdateProductAsync(SelectedProduct, StockAdjustment);

                App.NotificationService.Notify("Inventory updated successfully.");

                #endregion
            }
            else
            {
                #region Add New Product

                var newProduct = new Product
                {
                    EAN13Barcode = ScannedBarcode,
                    ProductName = ProductName,
                    BrandName = BrandName,
                    Category = SelectedCategory,
                    ProductWeight = ProductWeight,
                    CurrentStockLevel = StockAdjustment,
                    WholesalePrice = WholesalePrice,
                    Price = Price
                };

                await _databaseService.SaveProductAsync(newProduct);

                App.NotificationService.Notify("Product added successfully.");

                #endregion
            }

            ResetFields();
        }

        public void ResetFields()
        {
            SelectedProduct = null;
            ScannedBarcode = string.Empty;
            ProductName = string.Empty;
            BrandName = string.Empty;
            SelectedCategory = string.Empty;
            ProductWeight = string.Empty;
            StockReductionReason = string.Empty;
            CurrentStockLevel = 0;
            StockAdjustment = 0;
            WholesalePrice = 0;
            Price = 0;
            IsStockReduction = false;

            _initialProductName = string.Empty;
            _initialBrandName = string.Empty;
            _initialCategory = string.Empty;
            _initialProductWeight = string.Empty;
            _initialWholesalePrice = 0;
            _initialPrice = 0;

            OnPropertyChanged(nameof(IsStockReduction));
            OnPropertyChanged(nameof(ScannedBarcode));
            OnPropertyChanged(nameof(ProductName));
            OnPropertyChanged(nameof(BrandName));
            OnPropertyChanged(nameof(SelectedCategory));
            OnPropertyChanged(nameof(ProductWeight));
            OnPropertyChanged(nameof(CurrentStockLevel));
            OnPropertyChanged(nameof(StockAdjustment));
            OnPropertyChanged(nameof(WholesalePrice));
            OnPropertyChanged(nameof(Price));
        }

        public void ToggleCamera()
        {
            _isCameraOn = !_isCameraOn;
            OnPropertyChanged(nameof(IsCameraOn));
        }

        #endregion
    }
}
