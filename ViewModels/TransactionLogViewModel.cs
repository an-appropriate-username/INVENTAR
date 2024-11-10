using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using INVApp.Services;
using INVApp.Models;
using System.Windows.Input;

namespace INVApp.ViewModels
{
    public class TransactionLogViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;

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

        public TransactionLogViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            ApplyFilterCommand = new Command(async () => await LoadTransactionsAsync());
            ClearFilterCommand = new Command(ClearFilters);
            

            DateFrom = DateTime.Now.AddDays(-3);
            DateTo = DateTime.Now;
            LogsToShow = 10;
            Task.Run(async () => await LoadTransactionsAsync());
        }

        private async Task LoadTransactionsAsync()
        {
            var transactions = await _databaseService.GetTransactionsAsync(DateFrom, DateTo, LogsToShow);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                TransactionLogs.Clear();
            });

            foreach (var transaction in transactions)
            {
                var items = await _databaseService.GetTransactionItemsAsync(transaction.Id);

                // Debugging
                if (items == null || !items.Any())
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert("Debug Info", $"No items found for Transaction ID: {transaction.Id}.", "OK");
                    });
                }
                else { transaction.TransactionItems = items; }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    TransactionLogs.Add(transaction);
                });
            }
        }

        private async void ClearFilters()
        {
            DateFrom = DateTime.Now.AddDays(-3);
            DateTo = DateTime.Now;
            LogsToShow = 10;

            await LoadTransactionsAsync();
        }
    }
}
