using INVApp.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using INVApp.Models;

namespace INVApp.ViewModels
{
    public class AccountViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;

        private User _currentUser;
        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                if (_currentUser != value)
                {
                    _currentUser = value;
                    OnPropertyChanged(nameof(CurrentUser));
                }
            }
        }

        public ICommand EditProfileCommand { get; }
        public ICommand LogoutCommand { get; }

        public AccountViewModel(DatabaseService databaseService) 
        { 
            _databaseService = databaseService;

            CurrentUser = App.CurrentUser;

            // Commands
            EditProfileCommand = new Command(OnEditProfile);
            LogoutCommand = new Command(OnLogout);
        }

        private void OnEditProfile()
        {
            // Navigate to Edit Profile Page
        }

        private void OnLogout()
        {
            // Handle Logout
        }

        public void RefreshCurrentUser()
        {
            CurrentUser = App.CurrentUser;
        }
    }
}
