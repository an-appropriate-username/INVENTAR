using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using INVApp.Models;
using INVApp.Services;

namespace INVApp.ViewModels
{
    public class AuditTrailViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private int _productId;

        public ICommand ExitCommand { get; }
        public ICommand ApplyFilterCommand { get; }
        public ICommand ClearFilterCommand { get; }

        private string? productName;
        public string? ProductName
        {
            get => productName;
            set
            {
                productName = $"{value} Changelog";
                OnPropertyChanged();
            }
        }

        public ObservableCollection<InventoryLog> InventoryLogs { get; } = new ObservableCollection<InventoryLog>();

        // Filter properties
        private DateTime _dateFrom;
        public DateTime DateFrom
        {
            get => _dateFrom;
            set { _dateFrom = value; OnPropertyChanged(); }
        }

        private DateTime _dateTo;
        public DateTime DateTo
        {
            get => _dateTo;
            set { _dateTo = value; OnPropertyChanged(); }
        }

        private string? _selectedChangeType;
        public string? SelectedChangeType
        {
            get => _selectedChangeType;
            set { _selectedChangeType = value; OnPropertyChanged(); }
        }

        private int _logsToShow = 5;
        public int LogsToShow
        {
            get => _logsToShow;
            set { _logsToShow = value; OnPropertyChanged(); }
        }

        public AuditTrailViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            // Set defaults
            DateFrom = DateTime.Now.AddMonths(-1); 
            DateTo = DateTime.Now; 
            SelectedChangeType = "All";

            ExitCommand = new Command(Exit);
            ApplyFilterCommand = new Command(ExecuteFilter);
            ClearFilterCommand = new Command(ClearFilters);
        }

        public async Task LoadLogsAsync(int productId)
        {
            _productId = productId;

            // Fetch product details, check for null
            var logProduct = await _databaseService.GetProductByIDAsync(productId);
            if (logProduct == null)
            {
                ProductName = "Product not found";
                return;
            }

            ProductName = logProduct.ProductName;

            // Fetch inventory logs, check for null 
            var logs = await _databaseService.GetInventoryLogsAsync(productId);
            if (logs == null || !logs.Any())
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    InventoryLogs.Clear();
                });
                return;
            }

            var orderedLogs = logs.OrderByDescending(log => log.Timestamp);

            // Update the InventoryLogs collection directly in the UI thread 
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                InventoryLogs.Clear();
                foreach (var log in orderedLogs)
                {
                    InventoryLogs.Add(log);
                }
            });
        }

        public async Task LoadFilteredLogsAsync()
        {
            var logs = await _databaseService.GetInventoryLogsAsync(_productId);

            var filteredLogs = logs
                .Where(log => log.Timestamp >= DateFrom && log.Timestamp <= DateTo)
                .Where(log => SelectedChangeType == "All" || log.ChangeType == SelectedChangeType)
                .OrderByDescending(log => log.Timestamp)
                .Take(LogsToShow);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                InventoryLogs.Clear();
                foreach (var log in filteredLogs)
                {
                    InventoryLogs.Add(log);
                }
            });
        }

        private void ExecuteFilter(object parameter)
        {
            _ = LoadFilteredLogsAsync();
        }

        private void ClearFilters()
        {
            DateFrom = DateTime.Now.AddMonths(-1); 
            DateTo = DateTime.Now; 
            SelectedChangeType = "All"; 
            LogsToShow = 5;
            _ = LoadFilteredLogsAsync();
        }

        private async void Exit()
        {
            _ = await Application.Current.MainPage.Navigation.PopAsync();
        }

    }
}
