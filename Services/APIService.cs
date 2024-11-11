using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using INVApp.Models;
using Newtonsoft.Json;

namespace INVApp.Services
{
    public class APIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUri = "https://localhost:7276/api/";

        public APIService()
        {
            _httpClient = new HttpClient();
        }

        #region Product Methods

        // Retrieve all products
        public async Task<List<Product>> GetProductsFromApiAsync(int pageNumber, int pageSize = 30)
        {
            int offset = (pageNumber - 1) * pageSize;


            var response = await _httpClient.GetAsync($"{_baseUri}Product/maui/products?offset={offset}&limit={pageSize}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                // Deserialize the JSON content to a list of Product objects
                return JsonConvert.DeserializeObject<List<Product>>(content);
            }
            else
            {
                throw new Exception("Error fetching products from the API.");
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
            var query = $"{_baseUri}Product/maui/product/search?query={Uri.EscapeDataString(searchQuery)}";

            if (!string.IsNullOrEmpty(selectedCategory))
            {
                query += $"&category={Uri.EscapeDataString(selectedCategory)}";
            }

            var response = await _httpClient.GetAsync(query);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Product>>(content);
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

        // Save a transaction and associated items
        public async Task<bool> SaveTransactionAsync(Transaction transaction)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUri}Transaction", transaction);
            return response.IsSuccessStatusCode;
        }

        // Retrieve transactions within a date range
        public async Task<List<Transaction>> GetTransactionsAsync(DateTime dateFrom, DateTime dateTo, int count)
        {
            var response = await _httpClient.GetAsync($"{_baseUri}Transaction?dateFrom={dateFrom:O}&dateTo={dateTo:O}&count={count}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Transaction>>() ?? new List<Transaction>();
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
            var response = await _httpClient.GetAsync($"{_baseUri}Customer");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Customer>>() ?? new List<Customer>();
        }

        // Add a new customer
        public async Task<bool> AddCustomerAsync(Customer customer)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUri}Customer", customer);
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

    }
}