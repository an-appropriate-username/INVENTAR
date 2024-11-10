using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using INVApp.Models;
using INVApp.Services;
using INVApp.Views;
using INVApp.ViewModels;
using System.Windows.Input;
using System.ComponentModel;

namespace INVApp.ViewModels
{
    public class StockOverviewViewModel
    {
        // Databse
        private readonly DatabaseService _databaseService;

        private CancellationTokenSource _searchCancellationTokenSource;
        private const int SearchDelayMilliseconds = 300;

        // Filters
        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
                SearchProducts();
            }
        }
        private string? _selectedCategory;
        public string? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                SearchProducts();
            }
        }

        // Products and Categories
        private int _pageSize = 24; 
        private int _currentPage = 1;
        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<string> Categories { get; set; }

        // Commands
        public ICommand OpenProductDetailCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand OpenChangelogCommand { get; }
        public ICommand LoadMoreCommand { get; }


        public StockOverviewViewModel(DatabaseService databaseService, StockIntakeViewModel intakeViewModel)
        {
            _databaseService = databaseService;
            Products = new ObservableCollection<Product>();
            Categories = new ObservableCollection<string>();

            OpenProductDetailCommand = new Command<Product>(OpenProductDetail);
            OpenChangelogCommand = new Command<Product>(OpenChangelog);
            SearchCommand = new Command(SearchProducts);
            ClearFiltersCommand = new Command(ClearFilters);
            LoadMoreCommand = new Command(async () => await LoadNextPage());

            Task.Run(async () => await LoadCategories());
            Task.Run(async () => await LoadProductsPage(1));
        }

        // Products
        public async Task LoadProductsPage(int page)
        {
            var offset = (page - 1) * _pageSize;
            var productsPage = await _databaseService.GetProductsPagedAsync(offset, _pageSize);

            Products.Clear();
            foreach (var product in productsPage)
            {
                Products.Add(product);
            }
            _currentPage = page;
        }

        public async Task LoadNextPage()
        {
            _currentPage++;
            var offset = (_currentPage - 1) * _pageSize;
            var productsPage = await _databaseService.GetProductsPagedAsync(offset, _pageSize);

            if (!productsPage.Any())
            {
                _currentPage--;
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                foreach (var product in productsPage)
                {
                    if (!Products.Any(p => p.ProductID == product.ProductID)) // Stopping duplicates showing
                    {
                        Products.Add(product);
                    }
                }
            } );
            
        }

        // Categories
        public async Task LoadCategories()
        {
            var categoriesFromDb = await _databaseService.GetCategoriesAsync();

            await MainThread.InvokeOnMainThreadAsync(() => 
            {
                Categories.Clear();
                foreach (var category in categoriesFromDb)
                {
                    Categories.Add(category.CategoryName);
                }
            });
        }

        private async void OpenProductDetail(Product selectedProduct)
        {
            var detailPage = new ProductDetailsPopup(selectedProduct, this);

            if (detailPage.BindingContext is ProductDetailViewModel viewModel)
            {
                viewModel.ProductUpdated += async () => await LoadProductsPage(_currentPage);
            }

            await Application.Current.MainPage.Navigation.PushModalAsync(detailPage);
        }

        private async void OpenChangelog(Product selectedProduct)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var changelogPage = new AuditTrailPage();
                var changelogViewModel = new AuditTrailViewModel(_databaseService);

                await changelogViewModel.LoadLogsAsync(selectedProduct.ProductID);

                changelogPage.BindingContext = changelogViewModel;

                await Application.Current.MainPage.Navigation.PushAsync(changelogPage);
            });
        }

        // Filters
        public async void SearchProducts()
        {
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource = new CancellationTokenSource();
            var token = _searchCancellationTokenSource.Token;

            await Task.Delay(SearchDelayMilliseconds);

            if (token.IsCancellationRequested)
                return;

            var productsFromDb = await _databaseService.GetProductsAsync();

            var filteredProducts = productsFromDb
                .Where(p =>
                    (string.IsNullOrEmpty(SearchQuery) ||
                    p.ProductName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    p.BrandName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(SelectedCategory) || p.Category == SelectedCategory)
                );

            var limitedProducts = filteredProducts.Take(100).ToList();

            Products.Clear();
            foreach (var product in limitedProducts)
            {
                Products.Add(product);
            }
        }
        public async void ClearFilters()
        {
            SearchQuery = string.Empty;
            SelectedCategory = null;
            SearchProducts();
            await LoadCategories();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
