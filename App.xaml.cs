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

        public static User? CurrentUser { get; set; }

        #endregion

        #region Constructor

        public App()
        {
            InitializeComponent();

            // Initialize services
            DatabaseService = new DatabaseService();
            NotificationService = new NotificationService();
            DatabaseConfigService = new DatabaseConfigService(DatabaseService);

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
