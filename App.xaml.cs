using INVApp.Services;
using INVApp.Views;
using INVApp.Models;


namespace INVApp
{
    /// <summary>
    /// The main application class.
    /// </summary>
    public partial class App : Application
    {
        #region Services

        public static DatabaseService? DatabaseService { get; private set; }

        public static NotificationService? NotificationService { get; private set; }

        public static DatabaseConfigService? DatabaseConfigService { get; private set; }

        public static APIService? APIService { get; private set; }

        // Current User properties

        public static event Action? CurrentUserChanged;

        private static User? _currentUser;
        public static User? CurrentUser
        {
            get => _currentUser;
            set
            {
                if (_currentUser != value)
                {
                    _currentUser = value;
                    CurrentUserChanged?.Invoke(); // Trigger the event without arguments
                }
            }
        }

        public static event Action? UserCreated;
        public static void OnUserCreated() => UserCreated?.Invoke();

        #endregion

        #region Constructor

        public App()
        {
            InitializeComponent();

            // Initialize services
            DatabaseService = new DatabaseService();
            NotificationService = new NotificationService();
            DatabaseConfigService = new DatabaseConfigService(DatabaseService);
            APIService = new APIService();

            DatabaseService.InitializeAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    // Handle any initialization errors
                    Console.WriteLine(task.Exception);
                }
            });

            // Set the main page of the application
            MainPage = new AppShell();

        }

        #endregion
    }
}
    