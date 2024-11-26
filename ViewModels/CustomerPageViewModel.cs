using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using SkiaSharp;
using INVApp.Services;
using INVApp.Models;
using BarcodeStandard;
using System.Transactions;

namespace INVApp.ViewModels
{
    public class CustomerPageViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly APIService _apiService;

        public ObservableCollection<Customer>? Customers { get; set; } = new ObservableCollection<Customer>();

        // Properties for new customer fields
        private string _customerName;
        private string _barcode;
        private string _surname;
        private string _email;
        private string _phone;
        private bool _isMember;
        private bool _generateBarcodeImage;
        private Customer? _selectedCustomer;

        public string CustomerName
        {
            get => _customerName;
            set { _customerName = value; OnPropertyChanged(); }
        }

        public string Barcode
        {
            get => _barcode;
            set { _barcode = value; OnPropertyChanged(); }
        }

        public string Surname
        {
            get => _surname;
            set { _surname = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(); }
        }

        public bool IsMember
        {
            get => _isMember;
            set { _isMember = value; OnPropertyChanged(); }
        }

        public bool GenerateBarcodeImage 
        {
            get => _generateBarcodeImage;
            set { _generateBarcodeImage = value; OnPropertyChanged(); }
        }

        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set { _selectedCustomer = value; OnPropertyChanged(); }
        }

        public bool IsExpandedBasedOnPlatform
        {
            get
            {
                #if WINDOWS
                    return true;
                #elif ANDROID
                    return false;
                #else
                    return false;
                #endif
            }
        }

        // Commands for add and delete actions
        public ICommand AddCustomerCommand { get; }
        public ICommand DeleteCustomerCommand { get; }

        public CustomerPageViewModel(DatabaseService databaseService, APIService apiService)
        {
            _databaseService = databaseService;
            _apiService = apiService;
            AddCustomerCommand = new Command(async () => await AddCustomerAsync());
            DeleteCustomerCommand = new Command(async () => await DeleteCustomerAsync());

            _ = LoadCustomersAsync();
        }

        // Load existing customers
        private async Task LoadCustomersAsync()
        {
            var customers = await _apiService.GetCustomersAsync();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Customers.Clear();
                foreach (var customer in customers)
                {
                    Customers.Add(customer);
                }
            });
            
        }

        // Add new customer
        private async Task AddCustomerAsync()
        {
            int uniqueCustomerId = await GenerateUniqueCustomerIdAsync();
            string customerBarcode = await GenerateUniqueBarcodeAsync();

            Barcode = customerBarcode;

            var newCustomer = new Customer
            {
                CustomerName = CustomerName,
                //CustomerId = uniqueCustomerId,
                Barcode = customerBarcode,
                Surname = Surname,
                Email = Email,
                Phone = Phone,
                IsMember = IsMember
            };

            if (GenerateBarcodeImage)
            {
                #if WINDOWS || MACCATALYST
                await GenerateAndSaveBarcodeImage(customerBarcode, uniqueCustomerId);
                #endif
            }

            App.CurrentUser.CustomersAdded += 1;

            await _apiService.AddCustomerAsync(newCustomer);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Customers.Add(newCustomer);
                ClearFields();
            });
        }

        // Delete selected customer
        private async Task DeleteCustomerAsync()
        {
            if (SelectedCustomer != null)
            {
                // Confirm deletion and ask if the user wants to delete stored customer data
                bool deleteCustomerData = await App.Current.MainPage.DisplayAlert(
                    "Delete Customer",
                    "Do you also want to delete this customers stored barcode images?",
                    "Yes",
                    "No");

                if (deleteCustomerData)
                {
                    await _apiService.DeleteCustomerAsync(SelectedCustomer.Id);

                    #if WINDOWS || MACCATALYST
                    await DeleteCustomerFilesAsync(SelectedCustomer);
                    #endif

                    bool deleteCustomer = await App.Current.MainPage.DisplayAlert(
                    "Delete Customer",
                    "Are you sure you want to delete this customer?",
                    "Yes",
                    "No");

                    if (deleteCustomer)
                    {
                        // Proceed with deleting the customer from the database
                        await _databaseService.DeleteCustomerAsync(SelectedCustomer);

                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            Customers.Remove(SelectedCustomer);
                            ClearFields();
                        });
                    }
                }
            }
        }
        private async Task DeleteCustomerFilesAsync(Customer customer)
        {
            var imageDirectory = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Select location to save the loyalty card image"
            });

            string customerDirectoryName = $"{customer.CustomerId}_{customer.CustomerName.Replace(" ", "_")}";
            string customerDirectoryPath = Path.Combine(Path.GetDirectoryName(imageDirectory.FullPath), customerDirectoryName);

            if (Directory.Exists(customerDirectoryPath))
            {
                Directory.Delete(customerDirectoryPath, true);
            }
        }

        // Generate unique four digit ID
        private async Task<int> GenerateUniqueCustomerIdAsync()
        {
            int customerId;
            bool isUnique;

            do
            {
                customerId = new Random().Next(1000, 10000);
                var existingCustomer = await _databaseService.Table<Customer>()
                    .Where(c => c.CustomerId == customerId)
                    .FirstOrDefaultAsync();
                isUnique = existingCustomer == null;
            } while (!isUnique);

            return customerId;
        }
        private async Task GenerateAndSaveBarcodeImage(string customerBarcode, int uniqueCustomerId)
        {

            // Directory and filename
            string fileName = $"{uniqueCustomerId}_{CustomerName.Replace(" ", "_")}_card.png";
            string barcodeFileName = $"{uniqueCustomerId}_{CustomerName.Replace(" ", "_")}_barcode.png";

            // Barcode generator
            var barcode = new Barcode
            {
                IncludeLabel = true,
            };

            // Generate the barcode image
            var barcodeImage = barcode.Encode(BarcodeStandard.Type.Ean13, customerBarcode, SKColors.Black, SKColors.White, 450, 250);

            // Set up card dimensions
            int cardWidth = 1000;
            int cardHeight = 600;
            int padding = 50;
            int borderThickness = 5;

            using var loyaltyCardBitmap = new SKBitmap(cardWidth, cardHeight);
            using var canvas = new SKCanvas(loyaltyCardBitmap);

            // Draw a background
            using (var paint = new SKPaint())
            {
                paint.Color = SKColors.Gray; // Dark background color
                canvas.DrawRect(0, 0, cardWidth, cardHeight, paint);
            }

            // Draw a border
            using (var borderPaint = new SKPaint())
            {
                borderPaint.Style = SKPaintStyle.Stroke;
                borderPaint.Color = SKColors.Black;
                borderPaint.StrokeWidth = borderThickness;
                canvas.DrawRect(borderThickness / 2, borderThickness / 2, cardWidth - borderThickness, cardHeight - borderThickness, borderPaint);
            }

            // Define text paint for customer details
            var textPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = 30,
                IsAntialias = true,
            };

            // Define positions for labels and values
            float labelX = 50;
            float valueX = 400;

            // Draw name
            canvas.DrawText("Name:", labelX, 100, textPaint);
            canvas.DrawText(CustomerName, valueX, 100, textPaint);

            // Draw surname if available
            if (!string.IsNullOrWhiteSpace(Surname))
            {
                canvas.DrawText(Surname, valueX+150, 100, textPaint);
            }

            // Draw unique four-digit ID
            canvas.DrawText("ID:", labelX, 220, textPaint);
            canvas.DrawText(uniqueCustomerId.ToString(), valueX, 220, textPaint);

            // Draw the barcode on the loyalty card with a quiet zone
            float quietZone = 20;
            float barcodeX = (cardWidth - barcodeImage.Width - 2 * quietZone) / 2f; 
            float barcodeY = cardHeight - barcodeImage.Height - padding - quietZone; 

            // Draw the quiet zone
            canvas.DrawRect(barcodeX - quietZone, barcodeY - quietZone, barcodeImage.Width + 2 * quietZone, barcodeImage.Height + 2 * quietZone, new SKPaint { Color = SKColors.White });

            // Position and draw the barcode on the card
            canvas.DrawImage(barcodeImage, barcodeX, barcodeY);

            // Save the card image
            var fileResult = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Select location to save the loyalty card image"
            });

            // Create a directory for the customer images
            string customerDirectoryName = $"{uniqueCustomerId}_{CustomerName.Replace(" ", "_")}";
            string customerDirectoryPath = Path.Combine(Path.GetDirectoryName(fileResult.FullPath), customerDirectoryName);
            Directory.CreateDirectory(customerDirectoryPath);

            if (fileResult != null && !string.IsNullOrEmpty(fileResult.FullPath))
            {
                var filePath = Path.Combine(customerDirectoryPath, fileName);

                using var stream = File.Create(filePath);
                using var data = loyaltyCardBitmap.Encode(SKEncodedImageFormat.Png, 100);
                data.SaveTo(stream);
            }

            // Create a separate canvas for the barcode with a quiet zone
            int barcodeWidth = (int)(barcodeImage.Width + 2 * quietZone); 
            int barcodeHeight = (int)(barcodeImage.Height + 2 * quietZone); 

            using var barcodeBitmap = new SKBitmap(barcodeWidth, barcodeHeight);
            using var barcodeCanvas = new SKCanvas(barcodeBitmap);

            // Draw the quiet zone
            barcodeCanvas.DrawRect(0, 0, barcodeWidth, barcodeHeight, new SKPaint { Color = SKColors.White });

            // Draw the barcode in the center of the barcode canvas
            barcodeCanvas.DrawImage(barcodeImage, new SKPoint(quietZone, quietZone));

            var barcodePath = Path.Combine(customerDirectoryPath, barcodeFileName);
            using var barcodeStream = File.Create(barcodePath);
            using var barcodeData = barcodeBitmap.Encode(SKEncodedImageFormat.Png, 100);
            barcodeData.SaveTo(barcodeStream);
        }

        private async Task<string> GenerateUniqueBarcodeAsync()
        {
            const string prefix = "99"; 
            string barcode;
            bool isUnique;

            do
            {
                var randomPart = new Random().NextInt64(1000000000, 9999999999);
                var baseBarcode = $"{prefix}{randomPart}";

                var fullBarcode = baseBarcode + CalculateChecksumDigit(baseBarcode);

                var existingCustomer = await _databaseService.Table<Customer>()
                    .Where(c => c.Barcode == fullBarcode)
                    .FirstOrDefaultAsync();

                isUnique = existingCustomer == null;
                barcode = fullBarcode;

            } while (!isUnique);

            return barcode;
        }

        private int CalculateChecksumDigit(string baseBarcode)
        {
            int sum = 0;

            for (int i = 0; i < baseBarcode.Length; i++)
            {
                int digit = int.Parse(baseBarcode[i].ToString());
                sum += (i % 2 == 0) ? digit : digit * 3;
            }

            int mod = sum % 10;
            return (mod == 0) ? 0 : 10 - mod;
        }

        // Method to clear the input fields
        private void ClearFields()
        {
            CustomerName = Surname = Barcode = Email = Phone = string.Empty;
            IsMember = false;
            SelectedCustomer = null;
            GenerateBarcodeImage = false;
        }
    }
}
