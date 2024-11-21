using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using INVApp.Models;
using INVApp.Services;
using Microsoft.Maui.Dispatching;

namespace INVApp.ViewModels
{
    public class TransactionLogViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly APIService _apiService;

        public ObservableCollection<Transaction> TransactionLogs { get; } = new ObservableCollection<Transaction>();

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

        private int _logsToShow = 10;
        public int LogsToShow
        {
            get => _logsToShow;
            set { _logsToShow = value; OnPropertyChanged(); }
        }

        public ICommand ApplyFilterCommand { get; }
        public ICommand ClearFilterCommand { get; }

        public TransactionLogViewModel(DatabaseService databaseService, APIService apiService)
        {
            _databaseService = databaseService;
            _apiService = apiService;

            // Initialize commands
            ApplyFilterCommand = new Command(async () => await LoadTransactionsAsync());
            ClearFilterCommand = new Command(async () => await ClearFiltersAsync());

            // Set default filter values
            DateFrom = DateTime.Now.AddDays(-30);
            DateTo = DateTime.Now;

            // Load initial transactions
            Task.Run(LoadTransactionsAsync);
        }

        private async Task LoadTransactionsAsync()
        {
            try
            {
                var transactions = await _apiService.GetTransactionsAsync(DateFrom.ToUniversalTime(), DateTo.ToUniversalTime(), LogsToShow);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    TransactionLogs.Clear();
                    if (transactions != null)
                    {
                        foreach (var transaction in transactions)
                        {
                            TransactionLogs.Add(transaction);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading transactions: {ex.Message}");
            }
        }


        private async Task ClearFiltersAsync()
        {
            DateFrom = DateTime.Now.AddDays(-30);
            DateTo = DateTime.Now;
            LogsToShow = 10;

            await LoadTransactionsAsync();
        }
    }
}
