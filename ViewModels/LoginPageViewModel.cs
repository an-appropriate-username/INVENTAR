using INVApp.Models;
using INVApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace INVApp.ViewModels
{
    public class LoginPageViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;

        private string? _userId;
        private string? _firstName;
        private string? _lastName;
        private string? _email;
        private string? _password;
        private string? _passcode;

        private bool canLogin;
        private bool canCreateAdmin;

        public string? UserId
        {
            get => _userId;
            set { _userId = value; OnPropertyChanged(); }
        }

        public string? FirstName
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(); }
        }

        public string? LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(); }
        }

        public string? Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string? Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public string? Passcode
        {
            get => _passcode;
            set { _passcode = value; OnPropertyChanged(); }
        }

        public bool CanLogin
        {
            get => canLogin;
            set
            {
                if (canLogin != value)
                {
                    canLogin = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool CanCreateAdmin
        {
            get => canCreateAdmin;
            set
            {
                if (canCreateAdmin != value)
                {
                    canCreateAdmin = value;
                    OnPropertyChanged();
                }
            }
        }

        // Command to handle login logic
        public ICommand LoginCommand { get; }

        // Command to handle admin creation logic
        public ICommand CreateAdminCommand { get; }

        public LoginPageViewModel(DatabaseService databaseService)
        {

            _databaseService = databaseService;

            // Initialize commands
            LoginCommand = new Command(async () => await OnLogin());
            CreateAdminCommand = new Command(async () => await OnCreateAdmin());

            InitializePage();

        }

        private async void InitializePage()
        {
            bool adminExists = await _databaseService.IsAdminUserExistsAsync();
            CanLogin = adminExists;
            CanCreateAdmin = !adminExists;
        }

        private async Task OnLogin()
        {
            if (string.IsNullOrWhiteSpace(UserId) || string.IsNullOrWhiteSpace(Passcode))
            {
                await App.Current.MainPage.DisplayAlert("Error", "User ID and Passcode are required.", "OK");
                return;
            }

            // Validate User ID as integer
            if (!int.TryParse(UserId, out int userId))
            {
                await App.Current.MainPage.DisplayAlert("Error", "Invalid User ID.", "OK");
                return;
            }

            // Retrieve user from database
            var user = await _databaseService.GetUserByDigitsAsync(userId);
            if (user == null)
            {
                await App.Current.MainPage.DisplayAlert("Error", "User not found.", "OK");
                return;
            }

            // Compare passcode (no hashing for simplicity)
            if (Passcode != user.Passcode)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Incorrect passcode.", "OK");
                return;
            }

            // Successful login
            App.CurrentUser = user;
            user.LastLogin = DateTime.Now;
            await _databaseService.UpdateUserAsync(user);

            ClearFields();

            await App.Current.MainPage.DisplayAlert("Success", "Login successful.", "OK");
            Application.Current.MainPage = new AppShell();
        }

        private async Task OnCreateAdmin()
        {
            if (!canCreateAdmin)
                return;

            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(Passcode) || string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
            {
                await App.Current.MainPage.DisplayAlert("Error", "Please fill all required fields correctly.", "OK");
                return;
            }

            var adminUser = new User
            {
                UserId = await GenerateUniqueUserIdAsync(),
                Privilege = User.UserPrivilege.Admin,
                FirstName = FirstName,
                LastName = LastName, // Can be null
                Email = Email,
                Password = Password, 
                Passcode = Passcode,
                ItemsScanned = 0,
                CustomersAdded = 0,
                TransactionsClosed = 0,
                CreatedAt = DateTime.Now,
                LastLogin = DateTime.MinValue
            };

            await _databaseService.AddUserAsync(adminUser);

            App.CurrentUser = adminUser;

            // Clear fields
            ClearFields();

            await App.Current.MainPage.DisplayAlert("Success", "Admin user created and logged in successfully.", "OK");
            Application.Current.MainPage = new AppShell();
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

        private void ClearFields()
        {
            UserId = string.Empty;
            Passcode = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            Email = string.Empty;
            Password = string.Empty;
        }

    }
}
