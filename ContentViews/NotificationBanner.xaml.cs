namespace INVApp.ContentViews;

public partial class NotificationBanner : ContentView
{
	public NotificationBanner()
	{
		InitializeComponent();
    }

    public async void Show(string message)
    {
        NotificationLabel.Text = message;
        NotificationFrame.HeightRequest = 60;
        NotificationFrame.IsVisible = true;

        await Task.Delay(5000);

        NotificationFrame.IsVisible = false;
    }

    private void HideNotification()
    {
        NotificationFrame.HeightRequest = 0;
        NotificationFrame.IsVisible = false;
    }

    private void CloseNotificationClicked(object sender, EventArgs e)
    {
        HideNotification();
    }
}