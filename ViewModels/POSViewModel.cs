using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BarcodeStandard;
using INVApp.DTO;
using INVApp.Models;
using INVApp.NewFolder;
using INVApp.Services;
using ZXing.QrCode.Internal;

namespace INVApp.ViewModels
{
    public class POSViewModel : BaseViewModel
    {
        #region Fields
        private readonly DatabaseService _databaseService;
        private readonly APIService _apiService;
        private readonly TransactionService _transactionService;
        private readonly StockService _stockService;
        private readonly ReceiptService _receiptService;
        private string? _scannedBarcode;
        #endregion

        #region Properties

        public Product? SelectedProduct { get; set; }
        public string? ProductName { get; set; }
        public string? ProductWeight { get; set; }
        public string? Category { get; set; }
        public int CurrentStockLevel { get; set; }
        public decimal WholesalePrice { get; set; }
        public decimal Price { get; set; }

        private decimal _totalCashAmount;
        private int CurrentUserId;

        private decimal _discountPercentage;
        private decimal _totalDiscount;
        private decimal _totalAmountAfterDiscount;
        public decimal FinalTotal => TotalAmountAfterDiscount + GSTAmount;


        public ObservableCollection<CartItem> Cart { get; set; }
        public ObservableCollection<Customer> Customers { get; } = new ObservableCollection<Customer>();

        public IEnumerable<CartItem> CartReversed => Cart.Reverse();

        public decimal TotalAmount => GetTotalAmount();

        public ObservableCollection<string> PaymentMethods { get; } = new ObservableCollection<string>
        {
            "Cash",
            "CreditCard",
            "DebitCard",
            "MobilePayment", // e.g., Apple Pay, Google Pay
            "GiftCard",
            "BankTransfer"
        };

        private decimal _gstAmount;
        public decimal GSTAmount
        {
            get => _gstAmount;
            private set
            {
                if (_gstAmount != value)
                {
                    _gstAmount = value;
                    OnPropertyChanged(nameof(GSTAmount));
                    OnPropertyChanged(nameof(FinalTotal)); // Ensure FinalTotal updates
                }
            }
        }

        private decimal _gstRate;
        public decimal GSTRate
        {
            get => _gstRate;
            private set
            {
                if (_gstRate != value)
                {
                    _gstRate = value;
                    OnPropertyChanged(nameof(GSTRate));
                    CalculateGST();
                }
            }
        }


        public decimal DiscountPercentage
        {
            get => _discountPercentage;
            set
            {
                if (_discountPercentage != value)
                {
                    _discountPercentage = value;
                    OnPropertyChanged(nameof(DiscountPercentage));
                    CalculateDiscount();
                }
            }
        }

        public decimal TotalDiscount
        {
            get => _totalDiscount;
            private set
            {
                if (_totalDiscount != value)
                {
                    _totalDiscount = value;
                    OnPropertyChanged(nameof(TotalDiscount));
                }
            }
        }

        public decimal TotalAmountAfterDiscount
        {
            get => _totalAmountAfterDiscount;
            private set
            {
                if (_totalAmountAfterDiscount != value)
                {
                    _totalAmountAfterDiscount = value;
                    OnPropertyChanged(nameof(TotalAmountAfterDiscount));
                }
            }
        }

        private bool _isCameraVisible;
        public bool IsCameraVisible
        {
            get => _isCameraVisible;
            set
            {
                if (_isCameraVisible != value)
                {
                    _isCameraVisible = value;
                    OnPropertyChanged(nameof(IsCameraVisible));
                }
            }
        }

        private bool _isCashGivenVisible;
        public bool IsCashGivenVisible
        {
            get => _isCashGivenVisible;
            set
            {
                _isCashGivenVisible = value;
                OnPropertyChanged();
            }
        }

        private bool _isEftposVisible;
        public bool IsEftposVisible
        {
            get => _isEftposVisible;
            set
            {
                _isEftposVisible = value;
                OnPropertyChanged();
            }
        }

        public string? ScannedBarcode
        {
            get => _scannedBarcode;
            set
            {
                _scannedBarcode = value;
                OnPropertyChanged();

                // Check if the barcode length is exactly 13 characters
                if (!string.IsNullOrEmpty(_scannedBarcode) && _scannedBarcode.Length == 13)
                {
                    // Trigger AddProduct and clear the barcode field
                    AddProductCommand.Execute(null);
                    _scannedBarcode = string.Empty;
                    OnPropertyChanged(nameof(ScannedBarcode));
                }
            }
        }

        private string? _selectedPaymentMethod;
        public string? SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set
            {
                _selectedPaymentMethod = value;
                OnPropertyChanged();
                UpdateVisibility();
            }
        }

        private Customer? _selectedCustomer;
        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (_selectedCustomer != value)
                {
                    _selectedCustomer = value;
                    OnPropertyChanged(nameof(SelectedCustomer));
                }
            }
        }

        private string _cashGiven;
        public string? CashGiven
        {
            get => _cashGiven;
            set
            {
                _cashGiven = value;
                OnPropertyChanged();
                CalculateChange();
            }
        }

        private decimal _changeAmount;
        public decimal ChangeAmount
        {
            get => _changeAmount;
            set
            {
                _changeAmount = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalCashAmount
        {
            get => _totalCashAmount;
            set
            {
                _totalCashAmount = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands

        public ICommand AddProductCommand { get; }
        public ICommand RemoveProductFromCartCommand { get; }
        public ICommand CheckoutCommand { get; }

        public ICommand ApplyDiscountCommand { get; }
        public ICommand ClearDiscountCommand { get; }

        #endregion

        #region Constructor
        public POSViewModel(
            DatabaseService databaseService,
            APIService apiService,
            TransactionService transactionService,
            StockService stockService,
            ReceiptService receiptService)
        {
            _databaseService = databaseService;
            _apiService = apiService;
            _transactionService = transactionService;
            _stockService = stockService;
            _receiptService = receiptService;

            Cart = new ObservableCollection<CartItem>();

            AddProductCommand = new Command(async () => await AddProduct());
            RemoveProductFromCartCommand = new Command<CartItem>(RemoveProductFromCart);
            CheckoutCommand = new Command(Checkout);

            Cart.CollectionChanged += (sender, args) =>
            {
                OnPropertyChanged(nameof(TotalAmount));
                CalculateDiscount();
                CalculateGST(); // Keeps GST updated
            };

            IsCameraVisible = DeviceInfo.Platform == DevicePlatform.iOS || DeviceInfo.Platform == DevicePlatform.Android;

            LoadTaxSettingsAsync();
            LoadCustomersAsync();
        }
        #endregion

        #region Public Methods

        public async Task ProcessScannedBarcode(string barcode)
        {
            // Check the first two digits of the barcode
            if (barcode.Length >= 2 && barcode.Substring(0, 2) == "99")
            {
                // Check for customer account in the database
                var customer = await _apiService.GetCustomerByBarcodeAsync(barcode);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (customer != null)
                    {
                        App.NotificationService.Confirm($"Customer account found: {customer.CustomerName}");
                        SelectedCustomer = customer;
                        ScannedBarcode = barcode;
                    }
                    else
                    {
                        App.NotificationService.Notify("No customer account found for this barcode.");
                    }
                });
            }
            else
            {
                // Continue processing the scanned barcode as a product
                var product = await _apiService.GetProductByBarcodeAsync(barcode);
                if (product != null)
                {
                    SetProductDetails(product);
                    AddProductToCart(product);
                    // Play audio or notify user here if needed
                }
                else
                {
                    ScannedBarcode = barcode;
                    NotifyNoMatchFound();
                }
            }

            // Update product details for UI
            UpdateProductDetails();
        }

        private async Task AddProduct()
        {
            if (ScannedBarcode.Length >= 2 && ScannedBarcode.Substring(0, 2) == "99")
            {
                // Check for customer account in the database
                var customer = await _apiService.GetCustomerByBarcodeAsync(ScannedBarcode);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (customer != null)
                    {
                        App.NotificationService.Confirm($"Customer account found: {customer.CustomerName}");
                        SelectedCustomer = customer;
                    }
                    else
                    {
                        App.NotificationService.Notify("No customer account found for this barcode.");
                    }
                });
            }
            else if (!string.IsNullOrEmpty(ScannedBarcode))
            {
                var product = await _apiService.GetProductByBarcodeAsync(ScannedBarcode);
                if (product != null)
                {
                    AddProductToCart(product);
                }
                else
                {
                    NotifyNoMatchFound();
                }
            }
        }

        public void RemoveProductFromCart(CartItem cartItem)
        {
            var originalCartItem = Cart.FirstOrDefault(item => item.Product.ProductID == cartItem.Product.ProductID);
            if (originalCartItem != null)
            {
                Cart.Remove(originalCartItem);

                // Notify the UI
                OnPropertyChanged(nameof(CartReversed));
                OnPropertyChanged(nameof(TotalAmount));
                OnPropertyChanged(nameof(TotalAmountAfterDiscount));
                OnPropertyChanged(nameof(TotalDiscount));
                OnPropertyChanged(nameof(FinalTotal));

                CalculateDiscount();
                CalculateGST();


                NotifyProductRemoved(cartItem);
            }
        }

        #endregion

        #region Private Methods

        private void SetProductDetails(Product product)
        {
            SelectedProduct = product;
            ScannedBarcode = product.EAN13Barcode;
            ProductName = product.ProductName;
            Category = product.Category;
            ProductWeight = product.ProductWeight;
            CurrentStockLevel = product.CurrentStockLevel;
            WholesalePrice = product.WholesalePrice;
            Price = product.Price;
        }

        private void SetTransactionDetails(Transaction transaction, int customerId, string receipt)
        {
            transaction.DateTime = DateTime.Now;
            transaction.PaymentMethod = SelectedPaymentMethod;
            transaction.Discount = TotalDiscount;
            transaction.GServiceTax = GSTAmount;
            transaction.TotalAmount = FinalTotal;
            transaction.CustomerId = customerId;
            transaction.Receipt = receipt;
        }

            private void UpdateProductDetails()
        {
            OnPropertyChanged(nameof(ScannedBarcode));
            OnPropertyChanged(nameof(ProductName));
            OnPropertyChanged(nameof(ProductWeight));
            OnPropertyChanged(nameof(CurrentStockLevel));
            OnPropertyChanged(nameof(WholesalePrice));
            OnPropertyChanged(nameof(Price));
        }

        private void AddProductToCart(Product product)
        {
            var existingItem = Cart.FirstOrDefault(item => item.Product.ProductID == product.ProductID);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                var newItem = new CartItem
                {
                    Product = product,
                    ProductName = product.ProductName,
                    Barcode = product.EAN13Barcode,
                    Price = product.Price,
                    Quantity = 1
                };

                newItem.PropertyChanged += OnCartItemPropertyChanged;
                Cart.Add(newItem);
            }

            App.CurrentUser.ItemsScanned += 1;

            OnPropertyChanged(nameof(CartReversed));
            OnPropertyChanged(nameof(TotalAmount));
            OnPropertyChanged(nameof(TotalAmountAfterDiscount));

            CalculateDiscount();
            CalculateGST();
        }

        private async Task<(TransactionDto transaction, List<TransactionItemDto> items)> CreateTransactionDto()
        {
            if (SelectedCustomer == null || SelectedCustomer.Id <= 0)
                throw new InvalidOperationException("Invalid customer selected.");

            var currentUserId = App.CurrentUser?.Id ?? throw new InvalidOperationException("No user is currently signed in");

            var transactionDto = new TransactionDto
            {
                TransactionDate = DateTime.UtcNow,
                TotalAmount = FinalTotal,
                Discount = TotalDiscount,
                PaymentMethod = SelectedPaymentMethod ?? throw new ArgumentNullException(nameof(SelectedPaymentMethod)),
                TaxAmount = GSTAmount,
                UserId = App.CurrentUser.Id,
                CustomerId = SelectedCustomer.Id,
                Customer = new CustomerDto
                {
                    CustomerId = SelectedCustomer.Id,
                    FirstName = SelectedCustomer.CustomerName,
                    LastName = SelectedCustomer.Surname,
                    Email = SelectedCustomer.Email,
                    PhoneNumber = SelectedCustomer.PhoneNumber,
                    IsLoyaltyMember = SelectedCustomer.IsLoyaltyMember,
                    Barcode = SelectedCustomer.Barcode
                }
            };

            var transactionItems = Cart.Select(item => new TransactionItemDto
            {
                ProductId = item.Product.ProductID,
                ProductName = item.ProductName,  // Include the product name
                Quantity = item.Quantity,
                UnitPrice = item.Price,
                TotalPrice = item.TotalPrice
            }).ToList();

            return (transactionDto, transactionItems);
        }

        public async void Checkout()
        {
            try
            {
                Debug.WriteLine($"SelectedCustomer ID: {SelectedCustomer?.Id}");

                var (transactionDto, transactionItems) = await CreateTransactionDto();

                var isSuccess = await _transactionService.ProcessCheckout(
                    transactionDto,
                    transactionItems,
                    SelectedCustomer,
                    SelectedPaymentMethod);

                if (isSuccess)
                {
                    App.NotificationService.Confirm("Transaction completed successfully.");
                    ClearCartAndFields();
                }
                else
                {
                    App.NotificationService.Notify("Transaction failed. Please try again.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Checkout error: {ex.Message}");
                App.NotificationService.Notify("An error occurred during checkout.");
            }
            finally
            {
                OnPropertyChanged(nameof(TotalAmount));
            }
        }



        //private string GenerateReceipt(string customerName, int customerId)
        //{
        //    var receipt = new StringBuilder();
        //    receipt.AppendLine("Receipt");
        //    receipt.AppendLine("--------------------");
        //    foreach (var item in Cart)
        //    {
        //        receipt.AppendLine($"{item.ProductName} x {item.Quantity} @ {item.Price:C} = {item.TotalPrice:C}");
        //    }
        //    receipt.AppendLine("--------------------");
        //    receipt.AppendLine($"Total: {TotalAmount:C}");
        //    receipt.AppendLine($"Payment Method: {SelectedPaymentMethod}");
        //    receipt.AppendLine($"Customer: {customerName} (ID: {customerId})"); 
        //    receipt.AppendLine("Thank you for your purchase!");

        //    return receipt.ToString();
        //}

        private decimal GetTotalAmount()
        {
            return Cart.Sum(item => item.Product.Price * item.Quantity);
        }

        private void ApplyDiscount()
        {
            CalculateDiscount();
        }

        private void ClearDiscount()
        {
            DiscountPercentage = 0;
            CalculateDiscount();
        }

        private void CalculateDiscount()
        {
            if (DiscountPercentage > 0 && DiscountPercentage <= 100)
            {
                TotalDiscount = TotalAmount * (DiscountPercentage / 100);
                TotalAmountAfterDiscount = TotalAmount - TotalDiscount;
            }
            else
            {
                TotalDiscount = 0;
                TotalAmountAfterDiscount = TotalAmount;
            }

            OnPropertyChanged(nameof(TotalAmountAfterDiscount));
            OnPropertyChanged(nameof(TotalDiscount));
            OnPropertyChanged(nameof(FinalTotal)); 
            CalculateGST();
        }

        private void CalculateGST()
        {
            GSTAmount = TotalAmountAfterDiscount * (GSTRate / 100);
        }

        private void CalculateChange()
        {
            // Try to parse the CashGiven value
            if (decimal.TryParse(CashGiven, NumberStyles.Any, CultureInfo.InvariantCulture, out var cashGivenValue))
            {
                // Calculate the change
                var change = cashGivenValue - FinalTotal;

                // Multiply by 20 to scale up (e.g., 1.23 becomes 24.6)
                var scaledChange = change * 20;

                // Round up or down depending on the fractional part
                if (scaledChange % 1 <= 0.08m) // Use '0.08m' to make it a decimal
                {
                    // Round down
                    ChangeAmount = Math.Floor(scaledChange) / 20;
                }
                else
                {
                    // Round up
                    ChangeAmount = Math.Ceiling(scaledChange) / 20;
                }
            }
            else
            {
                ChangeAmount = 0;
            }
        }


        //private async Task DecrementStock()
        //{
        //    foreach (var item in Cart)
        //    {
        //        var product = item.Product;

        //        if (product != null)
        //        {
        //            product.CurrentStockLevel -= item.Quantity;
        //            if (product.CurrentStockLevel < 0) { product.CurrentStockLevel = 0; }
        //            await _databaseService.DecrementProductAsync(product);
        //        }
        //    }
        //}

        private async void LoadCustomersAsync()
        {
            var customers = await _apiService.GetCustomersAsync();
            Customers.Clear();

            Customers.Add(new Customer { Id = 0, CustomerId = 0, CustomerName = "Guest" });

            foreach (var customer in customers)
            {
                Customers.Add(customer); 
            }
        }

        private async void LoadTaxSettingsAsync()
        {
            var taxSettings = await _databaseService.GetTaxSettingsAsync();
            GSTRate = (decimal)taxSettings.GST; 
        }

        private void ClearCartAndFields()
        {
            // Clear the cart
            Cart.Clear();
            OnPropertyChanged(nameof(CartReversed));

            // Reset customer selection
            SelectedCustomer = null;
            OnPropertyChanged(nameof(SelectedCustomer));

            // Reset payment method
            SelectedPaymentMethod = null;
            OnPropertyChanged(nameof(SelectedPaymentMethod));

            // Reset financial fields
            CashGiven = string.Empty;
            ChangeAmount = 0;
            DiscountPercentage = 0;
            TotalDiscount = 0;
            TotalAmountAfterDiscount = 0;

            // Reset scanned barcode
            ScannedBarcode = string.Empty;
            OnPropertyChanged(nameof(ScannedBarcode));

            // Reset product details
            ProductName = null;
            ProductWeight = null;
            Category = null;
            CurrentStockLevel = 0;
            WholesalePrice = 0;
            Price = 0;

            // Notify the UI of reset
            OnPropertyChanged(nameof(TotalAmount));
            OnPropertyChanged(nameof(CashGiven));
            OnPropertyChanged(nameof(ChangeAmount));
            OnPropertyChanged(nameof(DiscountPercentage));
            OnPropertyChanged(nameof(TotalDiscount));
            OnPropertyChanged(nameof(TotalAmountAfterDiscount));
            OnPropertyChanged(nameof(ProductName));
            OnPropertyChanged(nameof(ProductWeight));
            OnPropertyChanged(nameof(Category));
            OnPropertyChanged(nameof(CurrentStockLevel));
            OnPropertyChanged(nameof(WholesalePrice));
            OnPropertyChanged(nameof(Price));
        }

        private void UpdateVisibility()
        {
            IsCashGivenVisible = SelectedPaymentMethod == "Cash";
            IsEftposVisible = SelectedPaymentMethod == "Eftpos";
        }

        private void NotifyNoMatchFound()
        {
            Application.Current.Dispatcher.Dispatch(() =>
            {
                App.NotificationService.Notify("No match found.");
            });
        }

        private void NotifyProductRemoved(CartItem cartItem)
        {
            Application.Current.Dispatcher.Dispatch(() =>
            {
                App.NotificationService.Confirm($"{cartItem.Product.ProductName} removed from cart.");
            });
        }

        private void OnCartItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CartItem.Quantity) || e.PropertyName == nameof(CartItem.TotalPrice))
            {
                OnPropertyChanged(nameof(TotalAmount));

                CalculateDiscount();
                CalculateGST();
            }
        }

        #endregion
    }
}