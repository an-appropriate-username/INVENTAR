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
        private readonly APIService _apiService;
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

        public StockIntakeViewModel(DatabaseService databaseService, APIService apiService)
        {
            _databaseService = databaseService;
            _apiService = apiService;

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
                if (_scannedBarcode != value)
                {
                    _scannedBarcode = value;
                    OnPropertyChanged();

                    // Only process if we have a valid barcode length and it's not empty
                    if (!string.IsNullOrEmpty(_scannedBarcode) && _scannedBarcode.Length == 13)
                    {
                        System.Diagnostics.Debug.WriteLine($"13-digit barcode entered: {_scannedBarcode}");
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            try
                            {
                                await ProcessScannedBarcode(_scannedBarcode);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error processing barcode: {ex}");
                                App.NotificationService.Notify($"Error processing barcode: {ex.Message}");
                            }
                        });
                    }
                }
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
            try
            {
                System.Diagnostics.Debug.WriteLine($"Starting ProcessScannedBarcode for barcode: {barcode}");

                if (string.IsNullOrEmpty(barcode))
                {
                    System.Diagnostics.Debug.WriteLine("Barcode is null or empty");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("Calling GetProductByBarcodeAsync...");
                var product = await _apiService.GetProductByBarcodeAsync(barcode);
                System.Diagnostics.Debug.WriteLine($"API call completed. Product found: {(product != null)}");

                // Toggle camera before processing results
                ToggleCamera();

                if (product != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Processing found product: {product.ProductName}");

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

                    System.Diagnostics.Debug.WriteLine("Product fields populated successfully");

                    // Notify user
                    Application.Current.Dispatcher.Dispatch(() =>
                    {
                        App.NotificationService.Notify($"Match found: {product.ProductName}.");
                    });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No product found, resetting fields");
                    ResetFields();
                    ScannedBarcode = barcode;
                    LoadDefaultCategory();

                    Application.Current.Dispatcher.Dispatch(() =>
                    {
                        App.NotificationService.Notify("No match found. Create new entry.");
                    });
                }

                System.Diagnostics.Debug.WriteLine("Updating UI properties");
                OnPropertyChanged(nameof(ScannedBarcode));
                OnPropertyChanged(nameof(ProductName));
                OnPropertyChanged(nameof(BrandName));
                OnPropertyChanged(nameof(SelectedCategory));
                OnPropertyChanged(nameof(ProductWeight));
                OnPropertyChanged(nameof(CurrentStockLevel));
                OnPropertyChanged(nameof(WholesalePrice));
                OnPropertyChanged(nameof(Price));
                System.Diagnostics.Debug.WriteLine("ProcessScannedBarcode completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ProcessScannedBarcode: {ex}");
                Application.Current.Dispatcher.Dispatch(() =>
                {
                    App.NotificationService.Notify($"Error scanning product: {ex.Message}");
                });
            }
        }

        public async Task LoadCategories()
        {
            var categories = await _apiService.GetCategoriesAsync();
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
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting AddStock method");
                System.Diagnostics.Debug.WriteLine($"ScannedBarcode: {ScannedBarcode}");
                System.Diagnostics.Debug.WriteLine($"SelectedProduct is null: {SelectedProduct == null}");

                #region Data Validation
                if (string.IsNullOrEmpty(ScannedBarcode) || string.IsNullOrEmpty(ProductName) ||
                    string.IsNullOrEmpty(SelectedCategory) || string.IsNullOrEmpty(ProductWeight))
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

                var success = false;

                if (SelectedProduct != null)
                {
                    System.Diagnostics.Debug.WriteLine("Updating existing product");
                    SelectedProduct.EAN13Barcode = ScannedBarcode;
                    System.Diagnostics.Debug.WriteLine($"Product details before update:");
                    System.Diagnostics.Debug.WriteLine($"- EAN13Barcode: {SelectedProduct.EAN13Barcode}");
                    System.Diagnostics.Debug.WriteLine($"- ProductName: {SelectedProduct.ProductName}");
                    System.Diagnostics.Debug.WriteLine($"- StockAdjustment: {StockAdjustment}");
                    SelectedProduct.ProductName = ProductName;
                    SelectedProduct.BrandName = BrandName;
                    SelectedProduct.Category = SelectedCategory;
                    SelectedProduct.ProductWeight = ProductWeight;
                    SelectedProduct.WholesalePrice = WholesalePrice;
                    SelectedProduct.Price = Price;
                    success = await _apiService.UpdateProductStockAsync(SelectedProduct, StockAdjustment);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Creating new product");
                    System.Diagnostics.Debug.WriteLine($"Details for new product:");
                    System.Diagnostics.Debug.WriteLine($"- Barcode: {ScannedBarcode}");
                    System.Diagnostics.Debug.WriteLine($"- Name: {ProductName}");
                    System.Diagnostics.Debug.WriteLine($"- StockAdjustment: {StockAdjustment}");

                    success = await _apiService.CreateProductStockAsync(
                        ScannedBarcode,
                        ProductName,
                        BrandName,
                        SelectedCategory,
                        ProductWeight,
                        WholesalePrice,
                        Price,
                        StockAdjustment
                    );
                }

                if (success)
                {
                    App.NotificationService.Notify("Inventory updated successfully.");
                    //ResetFields();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AddStock: {ex}");
                App.NotificationService.Notify($"Error updating inventory: {ex.Message}");
            }
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
