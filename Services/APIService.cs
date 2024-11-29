using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using INVApp.DTO;
using INVApp.Models;
using INVApp.NewFolder;
using Newtonsoft.Json;

namespace INVApp.Services
{
    public class APIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUri = "https://localhost:7276/api/";
        private string _initialProductName;
        private string _initialBrandName;
        private string _initialCategory;
        private decimal _initialWholesalePrice;
        private decimal _initialPrice;

        public APIService()
        {
            _httpClient = new HttpClient();
        }

        #region Product Methods

        // Retrieve all products
        public async Task<List<Product>> GetProductsFromApiAsync(int pageNumber, int pageSize = 30)
        {
            int offset = (pageNumber - 1) * pageSize;

            var response = await _httpClient.GetAsync($"{_baseUri}Maui/products?offset={offset}&limit={pageSize}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                // Deserialize to a wrapper object that matches the API structure
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content);

                // Map the products from the API response to your Product model
                var products = apiResponse.Products.Select(p => new Product
                {
                    ProductID = (int)p.id,
                    ProductName = (string)p.name,
                    BrandName = (string)p.brandName,
                    ProductWeight = (string)p.weight,
                    Category = (string)p.categoryName,
                    CurrentStockLevel = (int)p.currentStockLevel,
                    MinimumStockLevel = (int)p.minimumStockLevel,
                    Price = (decimal)p.price,
                    WholesalePrice = (decimal)p.wholesalePrice,
                    EAN13Barcode = (string)p.ean13Barcode
                }).ToList();

                return products;
            }
            else
            {
                throw new Exception("Error fetching products from the API.");
            }
        }

        public class ApiResponse
        {
            public int TotalCount { get; set; }
            public List<dynamic> Products { get; set; }
        }



        public async Task<Product> GetProductByBarcodeAsync(string barcode)
        {
            try
            {
                // Make the HTTP GET request
                var response = await _httpClient.GetAsync($"{_baseUri}Maui/Product/barcode/{barcode}");

                if (response.IsSuccessStatusCode)
                {
                    // Read and deserialize the response content
                    var content = await response.Content.ReadAsStringAsync();
                    var productJson = JsonConvert.DeserializeObject<dynamic>(content); // Use dynamic for JSON properties

                    // Transform the product
                    var product = new Product
                    {
                        ProductID = (int)productJson.id,
                        ProductName = (string)productJson.name,
                        BrandName = (string)productJson.brandName,
                        ProductWeight = (string)productJson.weight,
                        Category = (string)productJson.categoryName, // Set the correct category property
                        CurrentStockLevel = (int)productJson.currentStockLevel,
                        MinimumStockLevel = (int)productJson.minimumStockLevel,
                        Price = (decimal)productJson.price,
                        WholesalePrice = (decimal)productJson.wholesalePrice,
                        EAN13Barcode = (string)productJson.ean13Barcode
                    };

                    return product; // Return the transformed product
                }
                else
                {
                    throw new Exception($"Error fetching product: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                // Handle/log exceptions as needed
                throw new Exception($"Failed to retrieve product: {ex.Message}");
            }
        }



        // Retrieve a product by ID
        public async Task<Product?> GetProductByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUri}Product/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Product>();
            }
            return null;
        }




        // Insert or update a product
        public async Task<bool> SaveProductAsync(Product product)
        {
            HttpResponseMessage response;
            if (product.ProductID == 0)
            {
                response = await _httpClient.PostAsJsonAsync($"{_baseUri}Product", product);
            }
            else
            {
                response = await _httpClient.PutAsJsonAsync($"{_baseUri}Product/{product.ProductID}", product);
            }
            return response.IsSuccessStatusCode;
        }

        // Delete a product
        public async Task<bool> DeleteProductAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUri}Product/{id}");
            return response.IsSuccessStatusCode;
        }

        #endregion

        #region Category Methods

        // Retrieve all categories
        public async Task<List<Category>> GetCategoriesAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUri}Category/api/maui");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Category>>() ?? new List<Category>();
        }

        // Retrieve a category by name
        public async Task<Category?> GetCategoryByNameAsync(string categoryName)
        {
            var response = await _httpClient.GetAsync($"{_baseUri}Category/name/{categoryName}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Category>();
            }
            return null;
        }

        //Retrieve Product by Search
        public async Task<List<Product>> SearchProductsAsync(string searchQuery, string selectedCategory = null)
        {
            // Build the query string with optional category and search query parameters
            var query = $"{_baseUri}maui/product/search?query={Uri.EscapeDataString(searchQuery)}";

            if (!string.IsNullOrEmpty(selectedCategory))
            {
                query += $"&category={Uri.EscapeDataString(selectedCategory)}";
            }

            var response = await _httpClient.GetAsync(query);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                // Deserialize directly to an anonymous object matching the API structure
                var apiProducts = JsonConvert.DeserializeObject<List<dynamic>>(content);

                // Map anonymous object to your Product model
                var products = apiProducts.Select(p => new Product
                {
                    ProductID = (int)p.id,
                    ProductName = (string)p.name,
                    BrandName = (string)p.brandName,
                    ProductWeight = (string)p.weight,
                    Category = (string)p.categoryName, // Use categoryName from the API
                    CurrentStockLevel = (int)p.currentStockLevel,
                    MinimumStockLevel = (int)p.minimumStockLevel,
                    Price = (decimal)p.price,
                    WholesalePrice = (decimal)p.wholesalePrice,
                    EAN13Barcode = (string)p.ean13Barcode
                }).ToList();

                return products;

            }
            else
            {
                throw new Exception("Error searching for products.");
            }
        }



        // Insert or update a category
        public async Task<bool> SaveCategoryAsync(Category category)
        {
            HttpResponseMessage response;
            if (category.CategoryID == 0)
            {
                response = await _httpClient.PostAsJsonAsync($"{_baseUri}Category", category);
            }
            else
            {
                response = await _httpClient.PutAsJsonAsync($"{_baseUri}Category/{category.CategoryID}", category);
            }
            return response.IsSuccessStatusCode;
        }

        // Delete a category
        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUri}Category/{id}");
            return response.IsSuccessStatusCode;
        }

        #endregion

        #region Transaction Methods


        public async Task<HttpResponseMessage> SaveTransactionAsync(TransactionDto transactionDto)
        {
            try
            {
                Debug.WriteLine($"Starting transaction save for customer: {transactionDto.CustomerId}");
                var response = await _httpClient.PostAsJsonAsync($"{_baseUri}Maui/Transactions", transactionDto);

                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Response status: {response.StatusCode}");
                Debug.WriteLine($"Response content: {responseContent}");

                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveTransactionAsync error: {ex.Message}");
                throw;
            }
        }

        public async Task<HttpResponseMessage> SaveTransactionItemsAsync(int transactionId, List<TransactionItemDto> items)
        {
            try
            {
                Debug.WriteLine($"Saving {items.Count} items for transaction {transactionId}");
                var response = await _httpClient.PostAsJsonAsync(
                    $"{_baseUri}Maui/Transactions/{transactionId}/Items",
                    items);

                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Items save response status: {response.StatusCode}");
                Debug.WriteLine($"Items save response content: {responseContent}");

                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveTransactionItemsAsync error: {ex.Message}");
                throw;
            }
        }


        // Retrieve transactions within a date range
        public async Task<List<Transaction>> GetTransactionsAsync(DateTime dateFrom, DateTime dateTo, int count)
        {
            string url = $"{_baseUri}Maui/Transactions?dateFrom={dateFrom:O}&dateTo={dateTo:O}&count={count}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error fetching transactions: {response.ReasonPhrase}");
            }

            var content = await response.Content.ReadAsStringAsync();

            // Deserialize the JSON content to a list of Transaction objects
            return JsonConvert.DeserializeObject<List<Transaction>>(content);
        }


        // Retrieve transaction items by transaction ID
        public async Task<List<TransactionItem>> GetTransactionItemsAsync(int transactionId)
        {
            var response = await _httpClient.GetAsync($"{_baseUri}Transaction/{transactionId}/items");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<TransactionItem>>() ?? new List<TransactionItem>();
        }

        #endregion

        #region Customer Methods

        // Retrieve all customers
        public async Task<List<Customer>> GetCustomersAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUri}Maui/Customers");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            //return JsonConvert.DeserializeObject<List<Customer>>(content);
            return await response.Content.ReadFromJsonAsync<List<Customer>>();
        }

        // Add a new customer
        public async Task<bool> AddCustomerAsync(Customer customer)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUri}Maui/Customers", customer);
            return response.IsSuccessStatusCode;
        }

        // Delete a customer
        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUri}Customer/{id}");
            return response.IsSuccessStatusCode;
        }

        // Retrieve a customer by barcode
        public async Task<Customer?> GetCustomerByBarcodeAsync(string barcode)
        {
            var response = await _httpClient.GetAsync($"{_baseUri}Customer/barcode/{barcode}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Customer>();
            }
            return null;
        }

        // Update a customer
        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            var response = await _httpClient.PutAsJsonAsync($"{_baseUri}Customer/{customer.Id}", customer);
            return response.IsSuccessStatusCode;
        }

        #endregion

        #region Inventory Log Methods

        // Add an inventory log entry
        public async Task<bool> AddInventoryLogAsync(InventoryLog log)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUri}inventoryLogs", log);
            return response.IsSuccessStatusCode;
        }

        // Retrieve inventory logs for a specific product
        public async Task<List<InventoryLog>> GetInventoryLogsAsync(int productId)
        {
            var response = await _httpClient.GetAsync($"{_baseUri}inventoryLogs?productId={productId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<InventoryLog>>() ?? new List<InventoryLog>();
        }

        #endregion


        #region Stock Adjustment Methods
        public async Task<bool> UpdateProductStockAsync(Product product, int stockAdjustment)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Product details being sent:");
                System.Diagnostics.Debug.WriteLine($"- Product ID: {product.ProductID}");
                System.Diagnostics.Debug.WriteLine($"- Barcode: {product.EAN13Barcode}");
                System.Diagnostics.Debug.WriteLine($"- Name: {product.ProductName}");
                System.Diagnostics.Debug.WriteLine($"- Stock Adjustment: {stockAdjustment}");

                var adjustment = new StockAdjustmentDto
                {
                    Ean13Barcode = product.EAN13Barcode,
                    StockAdjustment = stockAdjustment,
                    Name = product.ProductName,
                    BrandName = product.BrandName,
                    CategoryName = product.Category,
                    Weight = product.ProductWeight,
                    WholesalePrice = product.WholesalePrice,
                    Price = product.Price
                };

                // Log the serialized DTO
                var jsonContent = JsonConvert.SerializeObject(adjustment);
                System.Diagnostics.Debug.WriteLine($"Sending JSON: {jsonContent}");

                var response = await _httpClient.PutAsJsonAsync(
                    $"{_baseUri}Maui/stock-adjustment",
                    adjustment
                );

                var responseContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Response Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                throw new Exception(responseContent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdateProductStockAsync: {ex}");
                throw;
            }
        }




        // Add this method to handle new products
        public async Task<bool> CreateProductStockAsync(
     string barcode,
     string productName,
     string brandName,
     string category,
     string weight,
     decimal wholesalePrice,
     decimal price,
     int stockAdjustment)
        {
            try
            {
                var adjustment = new StockAdjustmentDto
                {
                    Ean13Barcode = barcode,
                    StockAdjustment = stockAdjustment,  // Changed from NewStockValue
                    Name = productName,                  // Changed from ProductName
                    BrandName = brandName,
                    CategoryName = category,             // Changed from Category
                    Weight = weight,                     // Changed from ProductWeight
                    WholesalePrice = wholesalePrice,
                    Price = price
                    // Removed UserId as it's not in the API DTO
                    // Removed OldStockValue as it's not in the API DTO
                    // Removed NewStockValue as we now use StockAdjustment
                };

                var jsonContent = JsonConvert.SerializeObject(adjustment);
                System.Diagnostics.Debug.WriteLine($"Sending JSON: {jsonContent}"); // For debugging

                var response = await _httpClient.PutAsJsonAsync(
                    $"{_baseUri}Maui/stock-adjustment",
                    adjustment
                );

                var responseContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Response Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                throw new Exception($"Failed to create product: {responseContent}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CreateProductStockAsync: {ex}");
                throw new Exception($"Error creating product: {ex.Message}");
            }
        }


        //    public async Task<bool> AddInventoryAdjustmentLogAsync(StockAdjustmentDto adjustment)
        //{
        //    try
        //    {
        //        var log = new InventoryLog
        //        {
        //            ProductID = adjustment.ProductId,
        //            Barcode = adjustment.Barcode,
        //            StockNewValue = adjustment.StockAdjustment,
        //            Reason = adjustment.StockReductionReason ?? "Stock intake adjustment",
        //            DateChanged = DateTime.UtcNow
        //        };

        //        return await AddInventoryLogAsync(log);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Error logging inventory adjustment: {ex.Message}");
        //    }
        //}

        #endregion

    }
    
}