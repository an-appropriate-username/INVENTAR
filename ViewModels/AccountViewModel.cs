using INVApp.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using INVApp.Models;
using INVApp.Views;

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

        public ICommand OpenCreateUserPageCommand { get; }

        public AccountViewModel(DatabaseService databaseService) 
        { 
            _databaseService = databaseService;

            CurrentUser = App.CurrentUser;

            App.CurrentUserChanged += RefreshCurrentUser;

            // Commands
            EditProfileCommand = new Command(OnEditProfile);
            LogoutCommand = new Command(OnLogout);
            OpenCreateUserPageCommand = new Command(OpenCreateUserPage);
        }

        public async void OpenCreateUserPage()
        {
            var createUserPage = new CreateUserPage();

            await Application.Current.MainPage.Navigation.PushModalAsync(createUserPage);
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

        ~AccountViewModel()
        {
            // Unsubscribe to prevent memory leaks
            App.CurrentUserChanged -= RefreshCurrentUser;
        }
    }
}
