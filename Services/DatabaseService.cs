using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using INVApp.Models;
using System.Collections.ObjectModel;

namespace INVApp.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;
        private readonly ChangeLogService _changeLogService;

        public DatabaseService()
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "InventoryDB.db3");
            _database = new SQLiteAsyncConnection(dbPath);
            _changeLogService = new ChangeLogService(this);

            _database.CreateTableAsync<Product>().Wait();
            _database.CreateTableAsync<Category>().Wait();

            _database.CreateTableAsync<Transaction>().Wait();
            _database.CreateTableAsync<TransactionItem>().Wait();

            _database.CreateTableAsync<InventoryLog>().Wait();

            _database.CreateTableAsync<Customer>().Wait();
            _database.CreateTableAsync<User>().Wait();

            _database.CreateTableAsync<AudioSettings>().Wait();
            _database.CreateTableAsync<TaxSettings>().Wait();

            _database.CreateTableAsync<ToDoItem>().Wait();

        }

        #region Default Methods
        public async Task InitializeAsync()
        {
            await SeedDefaultProductsAsync();
            await SeedDefaultCategoryAsync();
        }

        public async Task SeedDefaultCategoryAsync()
        {
            var defaultCategory = new Category
            {
                CategoryName = "Default"
            };

            var existingCategory = await GetCategoryByNameAsync(defaultCategory.CategoryName);
            if (existingCategory == null)
            {
                await SaveCategoryAsync(defaultCategory);
            }
        }

        public async Task SeedDefaultProductsAsync()
        {
            var defaultProducts = new List<Product>
            {
            new Product { EAN13Barcode = "2001569676635", ProductName = "Custom Product 1", Price = 1.00m, BrandName = "Default", Category = "Default", ProductWeight = "0", CurrentStockLevel = 0, MinimumStockLevel = 0, WholesalePrice = 0 },
            new Product { EAN13Barcode = "2084699741176", ProductName = "Custom Product 2", Price = 2.00m, BrandName = "Default", Category = "Default", ProductWeight = "0", CurrentStockLevel = 0, MinimumStockLevel = 0, WholesalePrice = 0 },
            new Product { EAN13Barcode = "2030310108705", ProductName = "Custom Product 3", Price = 3.00m, BrandName = "Default", Category = "Default", ProductWeight = "0", CurrentStockLevel = 0, MinimumStockLevel = 0, WholesalePrice = 0},
            new Product { EAN13Barcode = "2074563495403", ProductName = "Custom Product 4", Price = 4.00m, BrandName = "Default", Category = "Default", ProductWeight = "0", CurrentStockLevel = 0, MinimumStockLevel = 0, WholesalePrice = 0},
            new Product { EAN13Barcode = "2062420397267", ProductName = "Custom Product 5", Price = 5.00m, BrandName = "Default", Category = "Default", ProductWeight = "0", CurrentStockLevel = 0, MinimumStockLevel = 0, WholesalePrice = 0},
            new Product { EAN13Barcode = "2094092383736", ProductName = "Custom Product 6", Price = 6.00m, BrandName = "Default", Category = "Default", ProductWeight = "0", CurrentStockLevel = 0, MinimumStockLevel = 0, WholesalePrice = 0},
            new Product { EAN13Barcode = "2058996473313", ProductName = "Custom Product 7", Price = 7.00m, BrandName = "Default", Category = "Default", ProductWeight = "0", CurrentStockLevel = 0, MinimumStockLevel = 0, WholesalePrice = 0},
            new Product { EAN13Barcode = "2064877694982", ProductName = "Custom Product 8", Price = 8.00m, BrandName = "Default", Category = "Default", ProductWeight = "0", CurrentStockLevel = 0, MinimumStockLevel = 0, WholesalePrice = 0}
            };

            foreach (var product in defaultProducts)
            {
                var existingProduct = await GetProductByBarcodeAsync(product.EAN13Barcode);
                if (existingProduct == null)
                {
                    await SaveProductAsync(product);
                }
            }
        }

        #endregion

        #region Product Methods

        // Insert or update a product
        public async Task<int> SaveProductAsync(Product product)
        {
            int result = 0;

            if (product.ProductID != 0)
            {
                result = await _database.UpdateAsync(product);
            }
            else
            {
                result = await _database.InsertAsync(product);

                var log = new InventoryLog
                {
                    ProductID = product.ProductID,
                    BrandNewValue = product.BrandName,
                    NameNewValue = product.ProductName,
                    CategoryNewValue = product.Category,
                    ChangeType = "Initial Add.",
                    StockOldValue = "0",
                    StockNewValue = product.CurrentStockLevel.ToString(),
                    WholesalePriceNewValue = product.Price,
                    PriceNewValue = product.Price,
                    //UserId = userId,
                    Timestamp = DateTime.Now
                };
                await AddInventoryLogAsync(log);

            }

            return result;
        }

        // Retrieve all products
        public Task<List<Product>> GetProductsAsync()
        {
            return _database.Table<Product>().ToListAsync();
        }

        public async Task<List<Product>> GetProductsPagedAsync(int offset, int limit)
        {
            return await _database.Table<Product>()
                .OrderBy(p => p.ProductName) 
                .Skip(offset)
                .Take(limit)
                .ToListAsync();
        }

        // Retrieve a specific product by ID
        public Task<Product> GetProductByIDAsync(int id)
        {
            return _database.Table<Product>().Where(i => i.ProductID == id).FirstOrDefaultAsync();
        }

        // Retrieve a specific product by barcode
        public Task<Product> GetProductByBarcodeAsync(string barcode)
        {
            return _database.Table<Product>().Where(i => i.EAN13Barcode == barcode).FirstOrDefaultAsync();
        }

        // Decrement Product 
        public async Task DecrementProductAsync(Product product)
        {
            var existingProduct = await _database.FindAsync<Product>(product.ProductID);

            if (existingProduct != null)
            {
                // Update existing product details
                existingProduct.CurrentStockLevel = product.CurrentStockLevel;

                await _database.UpdateAsync(existingProduct);
            }
        }

        // Update product
        public async Task UpdateProductAsync(Product product, int stockAdjustment)
        {
            // Check if the product exists in the database
            //var existingProduct = await GetProductByIDAsync(product.ProductID);
            var existingProduct = await _database.FindAsync<Product>(product.ProductID);

            // Create a deep copy of the existing product to capture the "before" state
            var _oldProduct = new Product
            {
                ProductID = existingProduct.ProductID,
                ProductName = existingProduct.ProductName,
                BrandName = existingProduct.BrandName,
                Category = existingProduct.Category,
                ProductWeight = existingProduct.ProductWeight,
                CurrentStockLevel = existingProduct.CurrentStockLevel,
                MinimumStockLevel = existingProduct.MinimumStockLevel,
                WholesalePrice = existingProduct.WholesalePrice,
                Price = existingProduct.Price
            };

            if (existingProduct != null)
            {
                var oldStockLevel = existingProduct.CurrentStockLevel;

                // Update existing product details
                existingProduct.ProductName = product.ProductName;
                existingProduct.BrandName = product.BrandName;
                existingProduct.Category = product.Category;
                existingProduct.ProductWeight = product.ProductWeight;
                existingProduct.CurrentStockLevel = product.CurrentStockLevel;
                existingProduct.MinimumStockLevel = product.MinimumStockLevel;
                existingProduct.WholesalePrice = product.WholesalePrice;
                existingProduct.Price = product.Price;

                await _database.UpdateAsync(existingProduct);

                // Log changes using ChangeLogService
                await _changeLogService.ProductUpdateChangeLogAsync(_oldProduct, existingProduct, stockAdjustment);
            }
        }

        // Delete a product
        public async Task<int> DeleteProductAsync(Product product)
        {
            await _database.Table<InventoryLog>()
                    .Where(log => log.ProductID == product.ProductID)
                    .DeleteAsync();

            return await _database.DeleteAsync(product);
        }

        #endregion

        #region Category Methods

        // Retrieve all categories
        public Task<List<Category>> GetCategoriesAsync()
        {
            return _database.Table<Category>().ToListAsync();
        }

        public Task<Category> GetCategoryByNameAsync(string categoryName)
        {
            return _database.Table<Category>().Where(c => c.CategoryName == categoryName).FirstOrDefaultAsync();
        }

        // Insert or update a category
        public Task<int> SaveCategoryAsync(Category category)
        {
            if (category.CategoryID != 0)
            {
                return _database.UpdateAsync(category);
            }
            else
            {
                return _database.InsertAsync(category);
            }
        }

        // Delete a category
        public Task<int> DeleteCategoryAsync(Category category)
        {
            return _database.DeleteAsync(category);
        }

        // Save default category
        public Task SaveDefaultCategoryAsync(string category)
        {
            Preferences.Set("DefaultCategory", category);
            return Task.CompletedTask;
        }

        // Load default category
        public Task<string?> GetDefaultCategoryAsync()
        {
            return Task.FromResult(Preferences.Get("DefaultCategory", string.Empty));
        }

        #endregion

        #region Audio Methods
        public async Task<AudioSettings> GetAudioSettingsAsync()
        {
            var settings = await _database.Table<AudioSettings>().FirstOrDefaultAsync();
            return settings ?? new AudioSettings(); // Return default settings if none exist
        }

        public async Task SaveAudioSettingsAsync(AudioSettings settings)
        {
            var existingSettings = await _database.Table<AudioSettings>().FirstOrDefaultAsync();
            if (existingSettings != null)
            {
                // Update existing settings
                settings.Id = existingSettings.Id;
                await _database.UpdateAsync(settings);
            }
            else
            {
                // Insert new settings
                await _database.InsertAsync(settings);
            }
        }

        #endregion

        #region Transaction Methods

        public async Task SaveTransactionAsync(Transaction transaction)
        {
            await _database.InsertAsync(transaction);

            // Save each transaction item, linking it to the transaction
            foreach (var item in transaction.TransactionItems)
            {
                item.TransactionId = transaction.Id; // Set foreign key
                await _database.InsertAsync(item);  // Save each TransactionItem
            }
        }

        public async Task<List<Transaction>> GetTransactionsAsync(DateTime dateFrom, DateTime dateTo, int count)
        {
            return await _database.Table<Transaction>()
                                .Where(t => t.DateTime >= dateFrom && t.DateTime <= dateTo)
                                .OrderByDescending(t => t.DateTime)
                                .Take(count)
                                .ToListAsync();
        }

        public async Task<ObservableCollection<TransactionItem>> GetTransactionItemsAsync(int transactionId)
        {
            var items = await _database.Table<TransactionItem>()
                                        .Where(item => item.TransactionId == transactionId)
                                        .ToListAsync();

            return new ObservableCollection<TransactionItem>(items);
        }

        #endregion

        #region User Methods
        public Task<List<User>> GetUsersAsync() => _database.Table<User>().ToListAsync();

        public Task<int> AddUserAsync(User user) => _database.InsertAsync(user);
        public Task<int> UpdateUserAsync(User user) => _database.UpdateAsync(user);

        public Task<int> DeleteUserAsync(User user) => _database.DeleteAsync(user);

        public Task<User> GetUserByDigitsAsync(int digits)
        {
            return _database.Table<User>().Where(c => c.UserId == digits).FirstOrDefaultAsync();
        }
        public async Task<bool> IsAdminUserExistsAsync()
        {
            int count = await _database.Table<User>()
                                .Where(u => u.Privilege == User.UserPrivilege.Admin)
                                .CountAsync();

            return count > 0; // If count > 0, at least one admin exists
        }

        #endregion

        #region Customer Methods
        public Task<List<Customer>> GetCustomersAsync() => _database.Table<Customer>().ToListAsync();

        public Task<int> AddCustomerAsync(Customer customer) => _database.InsertAsync(customer);

        public Task<int> DeleteCustomerAsync(Customer customer) => _database.DeleteAsync(customer);

        public Task<Customer> GetCustomerByBarcodeAsync(string barcode)
        {
            return _database.Table<Customer>().Where(c => c.Barcode == barcode).FirstOrDefaultAsync();
        }

        public AsyncTableQuery<T> Table<T>() where T : new()
        {
            return _database.Table<T>();
        }
        #endregion

        #region Log Methods
        public async Task AddInventoryLogAsync(InventoryLog log)
        {
            await _database.InsertAsync(log);
        }

        public Task<List<InventoryLog>> GetInventoryLogsAsync(int productId)
        {
            return _database.Table<InventoryLog>().Where(log => log.ProductID == productId).ToListAsync();
        }

        #endregion

        #region DatabaseConfig Methods

        // Clear all categories
        public async Task ClearCategoriesAsync()
        {
            await _database.DeleteAllAsync<Category>();
        }

        // Clear all products
        public async Task ClearProductsAsync()
        {
            await _database.DeleteAllAsync<Product>();
        }

        // Clear all logs
        public async Task ClearLogsAsync()
        {
            await _database.DeleteAllAsync<InventoryLog>();
        }

        // Clear all audio settings
        public async Task ClearAudioSettingsAsync()
        {
            await _database.DeleteAllAsync<AudioSettings>();
        }

        public async Task ClearTransactionsAsync()
        {
            await _database.DeleteAllAsync<Transaction>();
        }

        public async Task ClearTransactionItemsAsync()
        {
            await _database.DeleteAllAsync<TransactionItem>();
        }

        // Get Database directory
        public string GetDatabasePath()
        {
            return _database.DatabasePath;
        }

        #endregion

        #region ToDoItem Methods
        public async Task<int> SaveTodoItemAsync(ToDoItem Item)
        {
            int result = 0;

            // Check if it's an update (if it has an Id, assuming Id is 0 for new items)
            if (Item.Id != 0)
            {
                // Update existing todo item
                result = await _database.UpdateAsync(Item);
            }
            else
            {
                // Insert new todo item
                result = await _database.InsertAsync(Item);
            }

            return result;
        }

        public Task<List<ToDoItem>> GetTodoItemsAsync()
        {
            return _database.Table<ToDoItem>().ToListAsync();
        }

        public async Task<int> DeleteTodoItemAsync(ToDoItem todoItem)
        {
            return await _database.DeleteAsync(todoItem);
        }

        #endregion

        #region Tax Methods
        public async Task<TaxSettings> GetTaxSettingsAsync()
        {
            var settings = await _database.Table<TaxSettings>().FirstOrDefaultAsync();
            return settings ?? new TaxSettings(); // Return default settings if none exist
        }

        public async Task SaveTaxSettingsAsync(TaxSettings settings)
        {
            var existingSettings = await _database.Table<TaxSettings>().FirstOrDefaultAsync();
            if (existingSettings == null)
            {
                await _database.InsertAsync(settings);
            }
            else
            {
                existingSettings.GST = settings.GST;
                await _database.UpdateAsync(existingSettings);
            }
        }
        #endregion
    }
}
