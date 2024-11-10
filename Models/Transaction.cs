using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace INVApp.Models
{
    public class Transaction
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public DateTime DateTime { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal Discount { get; set; }

        public string? PaymentMethod { get; set; }

        public decimal GServiceTax { get; set; }

        public int? CustomerId { get; set; }

        public string? Receipt { get; set; }
        public ICommand ShowReceiptCommand => new Command(ShowReceipt);

        // instance of a user model. The user logged in when doing the transaction

        [Ignore]
        public ObservableCollection<TransactionItem> TransactionItems { get; set; } = new ObservableCollection<TransactionItem>();

        private async void ShowReceipt()
        {
            await Application.Current.MainPage.DisplayAlert("Transaction Receipt", Receipt, "OK");
        }

    }
}
