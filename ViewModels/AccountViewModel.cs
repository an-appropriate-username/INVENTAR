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
using System.Collections.ObjectModel;

namespace INVApp.ViewModels
{
    public class AccountViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;

        public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();

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
                    OnPropertyChanged(nameof(IsAdmin));
                }
            }
        }

        private User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (_selectedUser != value)
                {
                    _selectedUser = value;
                    OnPropertyChanged(nameof(SelectedUser));
                }
            }
        }

        public bool IsAdmin => CurrentUser?.Privilege == User.UserPrivilege.Admin;

        public ICommand EditProfileCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand OpenCreateUserPageCommand { get; }

        public AccountViewModel(DatabaseService databaseService) 
        { 
            _databaseService = databaseService;

            CurrentUser = App.CurrentUser;

            App.CurrentUserChanged += RefreshCurrentUser;
            App.UserCreated += LoadUsers;

            // Commands
            DeleteUserCommand = new Command(OnDeleteUser);
            EditProfileCommand = new Command(OnEditProfile);
            LogoutCommand = new Command(OnLogout);
            OpenCreateUserPageCommand = new Command(OpenCreateUserPage);

            LoadUsers();
        }

        public async void OpenCreateUserPage()
        {
            var createUserPage = new CreateUserPage();

            await Application.Current.MainPage.Navigation.PushModalAsync(createUserPage);
        }

        private void OnDeleteUser()
        {
            if (SelectedUser != null)
            {
                //Users.Remove(SelectedUser);
                //_databaseService.DeleteUser(SelectedUser.Id);
                //SelectedUser = null;
            }
        }

        public async void LoadUsers()
        {
            var usersFromDb = await _databaseService.GetUsersAsync();
            Users.Clear();

            foreach (var user in usersFromDb)
            {
                Users.Add(user);
            }
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
            App.UserCreated -= LoadUsers;
        }
    }
}
