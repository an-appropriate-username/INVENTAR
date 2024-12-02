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

		public static APIService? APIService { get; private set; }

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

			User? user = App.CurrentUser;
			var savedTheme = "Default";
			if (user != null && !string.IsNullOrEmpty(user.Theme))
			{
				savedTheme = user.Theme;
			}
			ApplyTheme(savedTheme);

			// Set the main page of the application
			MainPage = new AppShell();

		}

		public static void ApplyTheme(string theme)
		{
			if (Application.Current?.Resources == null) return;

			// You will need to copy your changes from this project into the other
			// project and continue working from there. 

			var resources = Application.Current.Resources;

			if (theme == "Dark")
			{
				resources["BackgroundGradientStart"] = Color.FromArgb("#1c1e22");
				resources["BackgroundGradientMiddle"] = Color.FromArgb("#0c0f13");
				resources["BackgroundGradientEnd"] = Color.FromArgb("#0a0c0f");

				resources["PrimaryColor"] = Color.FromArgb("#3a3f47");
				resources["SecondaryColor"] = Color.FromArgb("#5f84c0");
				resources["WarningColor"] = Color.FromArgb("#f87171");
				resources["BackgroundColor"] = Color.FromArgb("#121416");
				resources["EntryBackgroundColor"] = Color.FromArgb("#242c35");
				resources["FrameBackgroundColor"] = Color.FromArgb("#242c35");
				resources["TextColor"] = Color.FromArgb("#D3D3D3");
				resources["BtnBackgroundColor"] = Color.FromArgb("#232a33");
			}
			else if (theme == "Default")
			{
				resources["BackgroundGradientStart"] = Color.FromArgb("#607d8b");
				resources["BackgroundGradientMiddle"] = Color.FromArgb("#607d8b");
				resources["BackgroundGradientEnd"] = Color.FromArgb("#607d8b");

				resources["PrimaryColor"] = Color.FromArgb("#90a4ae");
				resources["SecondaryColor"] = Color.FromArgb("#eaf2f8");
				resources["WarningColor"] = Color.FromArgb("#d98880");
				resources["BackgroundColor"] = Color.FromArgb("#607d8b");
				resources["EntryBackgroundColor"] = Color.FromArgb("#cfd8dc");
				resources["FrameBackgroundColor"] = Color.FromArgb("#37474f");
				resources["TextColor"] = Color.FromArgb("#263238");
				resources["BtnBackgroundColor"] = Color.FromArgb("#1e293b");
			}
			else if (theme == "Blue")
			{
				resources["BackgroundGradientStart"] = Color.FromArgb("#9bafd9");
				resources["BackgroundGradientMiddle"] = Color.FromArgb("#162454");
				resources["BackgroundGradientEnd"] = Color.FromArgb("#101011");

				resources["PrimaryColor"] = Color.FromArgb("#161e2d");
				resources["SecondaryColor"] = Color.FromArgb("#1d4ed8");
				resources["WarningColor"] = Color.FromArgb("#ef4444");
				resources["BackgroundColor"] = Color.FromArgb("#1e293b");
				resources["EntryBackgroundColor"] = Color.FromArgb("#334155");
				resources["FrameBackgroundColor"] = Color.FromArgb("#334155");
				resources["TextColor"] = Color.FromArgb("#ffffff");
				resources["BtnBackgroundColor"] = Color.FromArgb("#1e293b");
			}
			else if (theme == "Light")// Light theme
			{
				resources["BackgroundGradientStart"] = Color.FromArgb("#ebf4f5");
				resources["BackgroundGradientMiddle"] = Color.FromArgb("#e0f2fe");
				resources["BackgroundGradientEnd"] = Color.FromArgb("#b5c6e0");

				resources["PrimaryColor"] = Color.FromArgb("#dcdcdc");
				resources["SecondaryColor"] = Color.FromArgb("#7dd3fc");
				resources["WarningColor"] = Color.FromArgb("#ef4444");
				resources["BackgroundColor"] = Color.FromArgb("#ffffff");
				resources["EntryBackgroundColor"] = Color.FromArgb("#f8fafc");
				resources["FrameBackgroundColor"] = Color.FromArgb("#f8fafc");
				resources["TextColor"] = Color.FromArgb("#0f172a");
				resources["BtnBackgroundColor"] = Color.FromArgb("#ffffff");
			}

			var navigation = Application.Current?.MainPage?.Navigation;
			if (navigation != null)
			{
				if (Application.Current.MainPage is Shell shell)
				{
					shell.BackgroundColor = (Color)resources["BackgroundColor"];
				}

				foreach (var page in navigation.NavigationStack)
				{
					if (page != null)
					{
						page.BackgroundColor = (Color)resources["BackgroundColor"];
					}
				}
			}

			Application.Current.MainPage?.Handler?.UpdateValue("BackgroundColor");
		}

		#endregion
	}
}
