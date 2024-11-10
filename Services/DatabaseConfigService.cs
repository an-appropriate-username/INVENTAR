using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using INVApp.ViewModels;
using INVApp.Models;

namespace INVApp.Services
{
    public class DatabaseConfigService
    {
        private readonly DatabaseService _databaseService;

        public DatabaseConfigService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        // Upload CSV File 

        public async Task UploadCsvFileAsync()
        {
            try
            {
                // Define allowed file types (for CSV)
                var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "public.comma-separated-values-text" } }, // iOS
                    { DevicePlatform.Android, new[] { "text/csv" } }, // Android
                    { DevicePlatform.WinUI, new[] { ".csv" } }, // Windows
                    { DevicePlatform.MacCatalyst, new[] { "public.comma-separated-values-text" } } // Mac
                });

                var options = new PickOptions
                {
                    PickerTitle = "Select a CSV file",
                    FileTypes = customFileType,
                };

                // Open File Picker
                var result = await FilePicker.Default.PickAsync(options);

                if (result == null)
                {
                    // If no file was picked
                    await Application.Current.MainPage.DisplayAlert("File Picker", "No file was selected.", "OK");
                    return;
                }

                await Application.Current.MainPage.DisplayAlert("File Picker", $"Selected file: {result.FileName}", "OK");

                // Open and read the file stream
                using (var stream = await result.OpenReadAsync())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var csvContent = await reader.ReadToEndAsync();

                        if (string.IsNullOrEmpty(csvContent))
                        {
                            await Application.Current.MainPage.DisplayAlert("Error", "The CSV file is empty.", "OK");
                            return;
                        }

                        await Application.Current.MainPage.DisplayAlert("File Content", $"File read successfully. Content size: {csvContent.Length} characters", "OK");

                        // Process the CSV data
                        await ProcessCsvData(csvContent);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle and display any errors
                await Application.Current.MainPage.DisplayAlert("Error", $"Something went wrong: {ex.Message}", "OK");
                Console.WriteLine($"Error uploading CSV: {ex.Message}");
            }
        }

        private async Task ProcessCsvData(string csvData)
        {
            var lines = csvData.Split(Environment.NewLine);
            if (lines.Length < 2)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "CSV contains no data rows.", "OK");
                return;
            }

            var uniqueCategories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];

                if (string.IsNullOrWhiteSpace(line)) continue;

                var columns = line.Split(',');

                if (columns.Length != 9)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Incorrect data format on line {i + 1}.", "OK");
                    continue;
                }

                try
                {
                    var category = columns[3].Trim();
                    if (!string.IsNullOrWhiteSpace(category))
                    {
                        uniqueCategories.Add(category);
                    }

                    // Create Product object from CSV row data
                    var newProduct = new Product
                    {
                        ProductName = columns[0].Trim(),
                        BrandName = columns[1].Trim(),
                        ProductWeight = columns[2].Trim(),
                        Category = category,
                        CurrentStockLevel = int.Parse(columns[4].Trim()),
                        MinimumStockLevel = int.Parse(columns[5].Trim()),
                        Price = decimal.Parse(columns[6].Trim()),
                        WholesalePrice = decimal.Parse(columns[7].Trim()),
                        EAN13Barcode = columns[8].Trim()
                    };

                    // Check if the product already exists in the database
                    var existingProduct = await _databaseService.GetProductByBarcodeAsync(newProduct.EAN13Barcode);
                    if (existingProduct != null)
                    {
                        newProduct.ProductID = existingProduct.ProductID;

                        // Calculate stock adjustment
                        int stockAdjustment = newProduct.CurrentStockLevel - existingProduct.CurrentStockLevel;

                        // Update the existing product
                        await _databaseService.UpdateProductAsync(newProduct, stockAdjustment);
                    }
                    else
                    {
                        // Save the new product to the database
                        await _databaseService.SaveProductAsync(newProduct);
                    }
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Error processing row {i + 1}: {ex.Message}", "OK");
                    continue;
                }
            }

            // Add categories to the database
            await AddCategoriesToDatabase(uniqueCategories);
            App.NotificationService.Notify("Success, CSV data processed successfully.");
        }

        private async Task AddCategoriesToDatabase(HashSet<string> categories)
        {
            // Fetch existing categories from the database
            var existingCategories = await _databaseService.GetCategoriesAsync();
            var existingCategoryNames = existingCategories.Select(c => c.CategoryName).ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Filter out already existing categories
            var newCategories = categories.Except(existingCategoryNames);

            // Add new categories to the database
            foreach (var category in newCategories)
            {
                var newCategory = new Category { CategoryName = category };
                await _databaseService.SaveCategoryAsync(newCategory);
            }

            if (newCategories.Any())
            {
                App.NotificationService.Notify($"New categories added: {string.Join(", ", newCategories)}");
            }
            else
            {
                App.NotificationService.Notify("No new categories to add.");
            }
        }

        // Danger Zone

        public async Task ResetDatabaseAsync()
        {
            bool isSure = await App.Current.MainPage.DisplayAlert("Warning", "Are you sure you want to reset the database?", "Yes", "No");
            if (!isSure) return;

            bool backupBeforeReset = await App.Current.MainPage.DisplayAlert("Backup", "Would you like to backup the database before reset?", "Yes", "No");

            if (backupBeforeReset)
            {
                await SetRestorePointAsync();
            }

            await _databaseService.ClearCategoriesAsync();
            await _databaseService.ClearProductsAsync();
            await _databaseService.ClearLogsAsync();
            await _databaseService.ClearAudioSettingsAsync();
            await _databaseService.ClearTransactionsAsync();
            await _databaseService.ClearTransactionItemsAsync();

            App.NotificationService.Notify("Success, Database has been reset.");
        }

        /// <summary>
        /// Sets a restore point by creating a backup of the current database.
        /// </summary>
        public async Task SetRestorePointAsync()
        {
            try
            {
                string internalBackupFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DatabaseBackups");

                if (!Directory.Exists(internalBackupFolderPath))
                {
                    Directory.CreateDirectory(internalBackupFolderPath);
                }

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFileName = $"DB_{timestamp}.db";

                string CurrentDatabaseFilePath = _databaseService.GetDatabasePath();
                string internalBackupFilePath = Path.Combine(internalBackupFolderPath, backupFileName);

                File.Copy(CurrentDatabaseFilePath, internalBackupFilePath, true);

                App.NotificationService.Notify($"Restore point created: {backupFileName}");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to create restore point: {ex.Message}", "OK");
            }
        }

        public async Task RestoreDatabaseFromBackupAsync(string backupFilePath)
        {
            try
            {
                string currentDatabaseFilePath = _databaseService.GetDatabasePath();

                // Backup the current database before restoring
                string backupFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DatabaseBackups");
                if (!Directory.Exists(backupFolderPath))
                {
                    Directory.CreateDirectory(backupFolderPath);
                }

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string currentDatabaseBackupPath = Path.Combine(backupFolderPath, $"DB_BackupBeforeRestore_{timestamp}.db");

                // Backup the current database file
                if (File.Exists(currentDatabaseFilePath))
                {
                    File.Copy(currentDatabaseFilePath, currentDatabaseBackupPath, overwrite: true);
                }

                // Copy the selected backup file to the current database location, restoring the database
                File.Copy(backupFilePath, currentDatabaseFilePath, overwrite: true);

                App.NotificationService.Notify("Success, database restored from backup.");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to restore database: {ex.Message}", "OK");
            }
        }
    }
}

