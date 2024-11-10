using INVApp.ViewModels;

namespace INVApp.Views;

public partial class AuditTrailPage : ContentPage
{
	public AuditTrailPage()
	{
		InitializeComponent();
		BindingContext = new AuditTrailViewModel(App.DatabaseService);
	}

}