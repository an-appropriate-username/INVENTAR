using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using INVApp.Models;
using INVApp.Services;

namespace INVApp.ViewModels
{
    public class CustomerDetailsViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly APIService _apiService;

        private Customer _selectedCustomer;

        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set { _selectedCustomer = value; OnPropertyChanged(); }
        }

        private ICommand UpdateCustomerCommand { get; }
        public ICommand DeleteCustomerCommand { get; }
        public ICommand CloseCommand { get; }

        // Constructor to initialize the selected customer
        public CustomerDetailsViewModel(Customer selectedCustomer, DatabaseService databaseService, APIService apiService)
        {
            _selectedCustomer = selectedCustomer;
            _databaseService = databaseService;
            _apiService = apiService;

            UpdateCustomerCommand = new Command(async () => await UpdateCustomerAsync());
            DeleteCustomerCommand = new Command(async () => await DeleteCustomerAsync());
            CloseCommand = new Command(ClosePopup);
        }

        private async Task UpdateCustomerAsync()
        {
            // Update the customer details in the database
            await _apiService.UpdateCustomerAsync(SelectedCustomer);
            await App.Current.MainPage.DisplayAlert("Success", "Customer details updated successfully.", "OK");
        }

        private async Task DeleteCustomerAsync()
        {
            // Confirm deletion
            bool confirm = await App.Current.MainPage.DisplayAlert("Delete Customer", "Are you sure you want to delete this customer?", "Yes", "No");
            if (confirm)
            {
                await _apiService.DeleteCustomerAsync(SelectedCustomer.Id);
                await App.Current.MainPage.DisplayAlert("Success", "Customer deleted successfully.", "OK");
                ClosePopup();
            }
        }

        private void ClosePopup()
        {
            // Close the modal popup
            Application.Current.MainPage.Navigation.PopModalAsync();
        }


    }
}
