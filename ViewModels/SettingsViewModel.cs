using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using INVApp.Services;
using INVApp.Models;
using INVApp.Views;

namespace INVApp.ViewModels
{
    /// <summary>
    /// ViewModel for handling settings related to categories, audio, and database configuration.
    /// </summary>
    public class SettingsViewModel : BaseViewModel
    {
		#region Fields and Services
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

		private readonly DatabaseService _databaseService;
        private readonly DatabaseConfigService _databaseConfigService;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the SettingsViewModel, setting up services and commands.
        /// </summary>
        public SettingsViewModel(DatabaseService databaseService, DatabaseConfigService databaseConfigService)
        {
            _databaseService = databaseService;
            _databaseConfigService = databaseConfigService;

			CurrentUser = App.CurrentUser;

			Categories = new ObservableCollection<Category>();
            BackupFrequencies = new ObservableCollection<string> { "Daily", "Bi-Daily", "Weekly", "Monthly", "Half-Yearly", "Yearly" };
            ArchiveFrequencies = new ObservableCollection<string> { "7 Days", "30 Days", "90 Days", "1 Year" };

            LoadCategoriesCommand = new Command(async () => await LoadCategories());
            AddCategoryCommand = new Command(async () => await AddCategory());
            RemoveCategoryCommand = new Command<Category>(async (category) => await RemoveCategory(category));
            SaveDefaultCategoryCommand = new Command(async () => await SaveDefaultCategory());

            SaveAudioSettingsCommand = new Command(async () => await SaveAudioSettings());
            LoadAudioSettingsCommand = new Command(async () => await LoadAudioSettings());

            ResetDatabaseCommand = new Command(async () => await ResetDatabase());
            UploadCsvCommand = new Command(async () => await UploadCsvFile());
            SetRestorePointCommand = new Command(async () => await SetRestorePointAsync());
            RestoreDatabaseCommand = new Command(async () => await  RestoreDatabaseAsync());

            SetThemeCommand = new Command<string>(async (theme) => await SetTheme(theme));
            CurrentTheme = !string.IsNullOrEmpty(CurrentUser.Theme) ? CurrentUser.Theme : "Default";

            _ = LoadDataAsync();
        }

        #endregion

        #region Theme Section

        private string _currentTheme;
		private bool _isDefaultTheme;
		private bool _isBlueTheme;
		private bool _isLightTheme;
        private bool _isDarkTheme;

        public bool IsDefaultTheme
        {
            get => _isDefaultTheme;
            set => SetProperty(ref _isDefaultTheme, value);
        }

        public bool IsBlueTheme
        {
            get => _isBlueTheme;
            set => SetProperty(ref _isBlueTheme, value);
        }

        public bool IsLightTheme
        {
            get => _isLightTheme;
            set => SetProperty(ref _isLightTheme, value);
        }

        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set => SetProperty(ref _isDarkTheme, value);
        }

        public string CurrentTheme
        {
            get => _currentTheme;
            set => SetProperty(ref _currentTheme, value);
        }

        public ICommand SetThemeCommand { get; }

        private async Task SetTheme(string theme)
        {
            if (theme == CurrentTheme) return;

            CurrentTheme = theme;
            Preferences.Set("AppTheme", theme);

            CurrentUser.Theme = theme;

            await _databaseService.UpdateUserAsync(CurrentUser);

            App.ApplyTheme(theme);

            IsDefaultTheme = theme == "Default";
            IsBlueTheme = theme == "Blue";
            IsLightTheme = theme == "Light";
            IsDarkTheme = theme == "Dark";
        }

		#endregion

		#region Categories Section

		/// <summary>
		/// List of categories.
		/// </summary>
		private ObservableCollection<Category> _categories;
        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged(nameof(Categories));
            }
        }

        /// <summary>
        /// The default category selected by the user.
        /// </summary>
        private Category? _defaultCategory;
        public Category? DefaultCategory
        {
            get => _defaultCategory;
            set
            {
                if (_defaultCategory != value)
                {
                    _defaultCategory = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The new category input field.
        /// </summary>
        private string _newCategory;
        public string NewCategory
        {
            get => _newCategory;
            set
            {
                _newCategory = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Loads the categories from the database.
        /// </summary>
        private async Task LoadCategories()
        {
            var categories = await _databaseService.GetCategoriesAsync();
            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
        }

        /// <summary>
        /// Loads the default category from the database.
        /// </summary>
        private async Task LoadDefaultCategory()
        {
            var categoryName = await _databaseService.GetDefaultCategoryAsync();
            if (Categories.Any())
            {
                DefaultCategory = Categories.FirstOrDefault(c => c.CategoryName == categoryName);
            }
        }

        /// <summary>
        /// Saves the default category to the database.
        /// </summary>
        private async Task SaveDefaultCategory()
        {
            if (DefaultCategory != null)
            {
                await _databaseService.SaveDefaultCategoryAsync(DefaultCategory.CategoryName);
            }
        }

        /// <summary>
        /// Adds a new category to the database.
        /// </summary>
        private async Task AddCategory()
        {
            if (string.IsNullOrWhiteSpace(NewCategory))
            {
                App.NotificationService.Notify("Category name cannot be empty.");
                return;
            }

            if (Categories.Any(c => string.Equals(c.CategoryName, NewCategory, StringComparison.OrdinalIgnoreCase)))
            {
                App.NotificationService.Notify("This category already exists.");
                return;
            }

            var category = new Category { CategoryName = NewCategory };
            await _databaseService.SaveCategoryAsync(category);

            Categories.Add(category);
            NewCategory = string.Empty;
        }

        /// <summary>
        /// Removes a category from the database.
        /// </summary>
        private async Task RemoveCategory(Category category)
        {
            if (Categories.Contains(category))
            {
                Categories.Remove(category);
                await _databaseService.DeleteCategoryAsync(category);

                App.NotificationService.Confirm($"Category {category.CategoryName} removed.");
                OnPropertyChanged(nameof(Categories));
                OnPropertyChanged(nameof(DefaultCategory));
            }
        }

        #region Commands

        public ICommand LoadCategoriesCommand { get; }
        public ICommand AddCategoryCommand { get; }
        public ICommand RemoveCategoryCommand { get; }
        public ICommand SaveDefaultCategoryCommand { get; }

        #endregion

        #endregion

        #region Audio Settings Section

        /// <summary>
        /// Volume setting for the application's sound effects.
        /// </summary>
        private double _soundVolume = 50.0; // Default volume
        public double SoundVolume
        {
            get => _soundVolume;
            set
            {
                if (_soundVolume != value)
                {
                    _soundVolume = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Flag indicating whether sound is enabled.
        /// </summary>
        private bool _isSoundEnabled;
        public bool IsSoundEnabled
        {
            get => _isSoundEnabled;
            set
            {
                if (_isSoundEnabled != value)
                {
                    _isSoundEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Loads the audio settings from the database.
        /// </summary>
        private async Task LoadAudioSettings()
        {
            var settings = await _databaseService.GetAudioSettingsAsync();
            SoundVolume = settings?.Volume ?? 50.0;
            IsSoundEnabled = settings?.IsEnabled ?? true;
        }

        /// <summary>
        /// Saves the audio settings to the database.
        /// </summary>
        private async Task SaveAudioSettings()
        {
            var settings = new AudioSettings
            {
                Volume = SoundVolume,
                IsEnabled = IsSoundEnabled
            };

            App.NotificationService.Confirm("Audio settings saved!");
            await _databaseService.SaveAudioSettingsAsync(settings);
        }

        #region Commands

        public ICommand SaveAudioSettingsCommand { get; }
        public ICommand LoadAudioSettingsCommand { get; }

        #endregion

        #endregion

        #region Auto Backup and Archive Section

        public ObservableCollection<string> BackupFrequencies { get; }
        public string SelectedBackupFrequency { get; set; }

        public ObservableCollection<string> ArchiveFrequencies { get; }
        public string SelectedArchiveFrequency { get; set; }

        private bool _isAutoBackupEnabled;
        public bool IsAutoBackupEnabled
        {
            get => _isAutoBackupEnabled;
            set
            {
                if (_isAutoBackupEnabled != value)
                {
                    _isAutoBackupEnabled = value;
                    OnPropertyChanged();

                    if (_isAutoBackupEnabled)
                    {
                        StartAutoBackup();
                    }
                    else
                    {
                        StopAutoBackup();
                    }
                }
            }
        }

        private bool _isAutoArchiveLogsEnabled;
        public bool IsAutoArchiveLogsEnabled
        {
            get => _isAutoArchiveLogsEnabled;
            set
            {
                if (_isAutoArchiveLogsEnabled != value)
                {
                    _isAutoArchiveLogsEnabled = value;
                    OnPropertyChanged();

                    if (_isAutoArchiveLogsEnabled)
                    {
                        StartAutoArchiving();
                    }
                    else
                    {
                        StopAutoArchiving();
                    }
                }
            }
        }

        private void StartAutoBackup()
        {
            App.NotificationService.Confirm("Auto backup enabled!");
        }

        private void StopAutoBackup()
        {
            App.NotificationService.Notify("Auto backup disabled!");
        }

        private void StartAutoArchiving()
        {
            App.NotificationService.Confirm("Auto archive enabled!");
        }

        private void StopAutoArchiving()
        {
            App.NotificationService.Notify("Auto archive disabled!");
        }

        #endregion

        #region Reset, Restore and CSV Section

        /// <summary>
        /// Resets the entire database.
        /// </summary>
        private async Task ResetDatabase()
        {
            Categories.Clear();
            await _databaseConfigService.ResetDatabaseAsync();
        }

        /// <summary>
        /// Uploads a CSV file to add or update products in the database.
        /// </summary>
        private async Task UploadCsvFile()
        {
            await _databaseConfigService.UploadCsvFileAsync();
            await Task.Delay(500); // Brief delay for file processing
            LoadCategories();
        }

        private async Task SetRestorePointAsync()
        {
            await _databaseConfigService.SetRestorePointAsync();
        }

        private async Task RestoreDatabaseAsync()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var restorePointPage = new RestorePointPage();
                var restorePointViewModel = new RestorePointViewModel(_databaseConfigService);

                await restorePointViewModel.LoadRestorePointsAsync(); 

                restorePointPage.BindingContext = restorePointViewModel;

                await Application.Current.MainPage.Navigation.PushAsync(restorePointPage);
            });
        }

        #region Commands

        public ICommand ResetDatabaseCommand { get; }
        public ICommand UploadCsvCommand { get; }
        public ICommand SetRestorePointCommand { get; }
        public ICommand RestoreDatabaseCommand { get; }

        #endregion

        #endregion

        #region Load All Data

        /// <summary>
        /// Calls all the methods related to loading data for the viewmodel. 
        /// </summary>
        private async Task LoadDataAsync()
        {
            await LoadCategories();
            await LoadDefaultCategory();
            await LoadAudioSettings();
        }

        #endregion
    }
}
