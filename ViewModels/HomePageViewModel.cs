using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using INVApp.Views;
using INVApp.Services;
using INVApp.Models;

namespace INVApp.ViewModels
{
    public class HomePageViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        public ObservableCollection<CarouselItem> CarouselItems { get; set; }
        public ObservableCollection<CarouselItem> SupportItems { get; set; }

        public ObservableCollection<ToDoItem> TodoItems { get; set; }

        public ICommand AddNewTodoCommand { get; }
        public ICommand SaveTodoCommand { get; }
        public ICommand DeleteTodoCommand { get; }
        public ICommand NavigateCommand { get; }

        public HomePageViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            NavigateCommand = new Command<string>(NavigateToPage);
            SaveTodoCommand = new Command<ToDoItem>(SaveTodoItem);
            AddNewTodoCommand = new Command(AddNewTodoItem);
            DeleteTodoCommand = new Command<ToDoItem>(DeleteTodoItem);

            TodoItems = new ObservableCollection<ToDoItem>();

            CarouselItems = new ObservableCollection<CarouselItem>
            {
                new CarouselItem { Title = "Point Of Sale", Icon = "pos_icon.png", OnClick = ShowAlert, NavigationPage = nameof(POSPage) },
                new CarouselItem { Title = "Transaction Logs", Icon = "log_icon.png", OnClick = ShowAlert, NavigationPage = nameof(TransactionLogPage) },
                new CarouselItem { Title = "Stock Adjustment", Icon = "stock_take_icon.png", OnClick = ShowAlert, NavigationPage = nameof(StockIntakePage) },
                new CarouselItem { Title = "Inventory", Icon = "overview_icon.png", OnClick = ShowAlert, NavigationPage = nameof(StockOverviewPage) },
            };

            SupportItems = new ObservableCollection<CarouselItem>
            {
                new CarouselItem { Title = "Settings", Icon = "settings_icon.png", OnClick = ShowAlert, NavigationPage = nameof(SettingsPage) }
            };

            LoadTodoItems();
        }

        public class CarouselItem
        {
            public string Title { get; set; }
            public string Icon { get; set; }
            public string NavigationPage { get; set; }
            public Action<string> OnClick { get; set; }
        }

        private async void NavigateToPage(string pageName)
        {
            if (!string.IsNullOrWhiteSpace(pageName))
            {
                await Shell.Current.GoToAsync(pageName);
            }
        }

        private async void LoadTodoItems()
        {
            var savedItems = await _databaseService.GetTodoItemsAsync();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                TodoItems.Clear();
                foreach (var item in savedItems)
                {
                    TodoItems.Add(item);
                }
            });
        }

        private async void AddNewTodoItem()
        {
            if (TodoItems.Count >= 8)
            {
                await Application.Current.MainPage.DisplayAlert("Limit Reached", "You can only have up to 8 tasks.", "OK");
                return; 
            }

            var newTodoItem = new ToDoItem
            {
                Task = string.Empty, 
                IsCompleted = false,
                CreatedAt = DateTime.Now
            };

            MainThread.BeginInvokeOnMainThread(() =>
            {
                TodoItems.Add(newTodoItem);
            });
        }

        private async void SaveTodoItem(ToDoItem item)
        {
            if (string.IsNullOrWhiteSpace(item?.Task)) return;
            await _databaseService.SaveTodoItemAsync(item);
        }

        private async void DeleteTodoItem(ToDoItem item)
        {
            if (item == null) return;

            await _databaseService.DeleteTodoItemAsync(item);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                TodoItems.Remove(item);
            });
        }

        private async void ShowAlert(string title)
        {
            var carouselItem = CarouselItems.FirstOrDefault(item => item.Title == title);
            if (carouselItem != null)
            {
                await Shell.Current.GoToAsync(carouselItem.NavigationPage);
            }
        }

    }
}

