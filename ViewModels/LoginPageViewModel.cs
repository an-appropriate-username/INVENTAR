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

        private bool canLogin;
        private bool canCreateAdmin;

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

        public LoginPageViewModel(DatabaseService databaseService) {

            _databaseService = databaseService;

            // Initialize commands
            LoginCommand = new Command(OnLogin);
            CreateAdminCommand = new Command(OnCreateAdmin);

            InitializePage();

        }

        private async void InitializePage()
        {
            bool adminExists = await _databaseService.IsAdminUserExistsAsync();
            CanLogin = adminExists;
            CanCreateAdmin = !adminExists;
        }

        private void OnLogin()
        {
            // Handle login logic
        }

        private void OnCreateAdmin()
        {
            // Handle admin creation logic
        }

    }
}
