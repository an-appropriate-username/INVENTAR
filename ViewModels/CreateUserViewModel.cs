using INVApp.Services;
using INVApp.Models;
using INVApp.Interfaces;
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
        private readonly IValidationService _validationService;

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

        public CreateUserViewModel(DatabaseService databaseService, IValidationService validationService) 
        {
            _databaseService = databaseService;
            _validationService = validationService;

            SaveCommand = new Command(SaveUser);
            CloseCommand = new Command(CloseModal);
        }

        #region Command Methods

        private async void SaveUser()
        {
            // Validate FirstName using ValidationService
            if (!_validationService.ValidateName(FirstName))
            {
                App.NotificationService.Notify("First Name must be 2-40 characters and contain only letters, spaces, or hyphens.");
                return;
            }

            // Validate LastName using ValidationService
            if (!_validationService.ValidateName(LastName))
            {
                App.NotificationService.Notify("Last Name must be 2-40 characters and contain only letters, spaces, or hyphens.");
                return;
            }

            // Validate Email using ValidationService
            if (!_validationService.ValidateEmail(Email))
            {
                App.NotificationService.Notify("Please enter a valid Email address.");
                return;
            }

            // Validate Password using ValidationService
            if (!_validationService.ValidatePassword(Password))
            {
                App.NotificationService.Notify("Password must be 8-64 characters, minimum one number, one uppercase letter, and one lowercase letter");
                return;
            }

            // Validate Passcode
            if (!_validationService.ValidatePasscode(Passcode))
            {
                App.NotificationService.Notify("Passcode must be at least 4 digits and contain only numbers.");
                return;
            }

            // Validate Privilege selection
            if (SelectedPrivilegeIndex < 0)
            {
                App.NotificationService.Notify("Please select a Privilege level.");
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

            App.OnUserCreated();

            // Notify success and close the modal
            App.NotificationService.Confirm("Success: New user created successfully!");
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
