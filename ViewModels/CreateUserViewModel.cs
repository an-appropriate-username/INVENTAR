using INVApp.Services;
using INVApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace INVApp.ViewModels
{
    public class CreateUserViewModel : BaseViewModel
    {
        #region Properties

        private readonly DatabaseService _databaseService;

        // Individual properties for each field
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Passcode { get; set; } = string.Empty;

        private int _selectedPrivilegeIndex;
        public int SelectedPrivilegeIndex
        {
            get => _selectedPrivilegeIndex;
            set
            {
                _selectedPrivilegeIndex = value;
                OnPropertyChanged(nameof(SelectedPrivilegeIndex));
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CloseCommand { get; }

        #endregion

        public CreateUserViewModel(DatabaseService databaseService) 
        {
            _databaseService = databaseService;

            SaveCommand = new Command(SaveUser);
            CloseCommand = new Command(CloseModal);
        }

        #region Command Methods

        private async void SaveUser()
        {
            // Validate FirstName
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                await Application.Current.MainPage.DisplayAlert("Validation Failed", "First Name cannot be empty.", "OK");
                return;
            }

            // Validate Email
            if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
            {
                await Application.Current.MainPage.DisplayAlert("Validation Failed", "Please enter a valid Email address.", "OK");
                return;
            }

            // Validate Password
            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 6)
            {
                await Application.Current.MainPage.DisplayAlert("Validation Failed", "Password must be at least 6 characters long.", "OK");
                return;
            }

            // Validate Passcode
            if (string.IsNullOrWhiteSpace(Passcode) || Passcode.Length < 4)
            {
                await Application.Current.MainPage.DisplayAlert("Validation Failed", "Passcode must be 4 or more digits long.", "OK");
                return;
            }

            // Validate Privilege selection
            if (SelectedPrivilegeIndex < 0)
            {
                await Application.Current.MainPage.DisplayAlert("Validation Failed", "Please select a Privilege level.", "OK");
                return;
            }

            // Create a new User instance
            var newUser = new User
            {
                UserId = await GenerateUniqueUserIdAsync(),
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                Password = Password,
                Passcode = Passcode,
                Privilege = (User.UserPrivilege)(SelectedPrivilegeIndex + 1),
                ItemsScanned = 0,
                CustomersAdded = 0,
                TransactionsClosed = 0,
                CreatedAt = DateTime.Now,
                LastLogin = DateTime.MinValue
            };

            // Save the user to the database
            await _databaseService.AddUserAsync(newUser);

            // Notify success and close the modal
            await Application.Current.MainPage.DisplayAlert("Success", "New user created successfully!", "OK");
            await Application.Current.MainPage.Navigation.PopModalAsync();
        }

        private async void CloseModal()
        {
            await Application.Current.MainPage.Navigation.PopModalAsync();
        }

        public async Task<int> GenerateUniqueUserIdAsync()
        {
            Random random = new Random();
            int userId;

            do
            {
                userId = random.Next(10000, 99999);
            }
            while (await IsUserIdTakenAsync(userId));

            return userId;
        }

        private async Task<bool> IsUserIdTakenAsync(int userId)
        {
            var existingUser = await _databaseService.GetUserByDigitsAsync(userId);
            return existingUser != null; // Returns true if a user with the same UserId exists
        }

        #endregion
    }
}
