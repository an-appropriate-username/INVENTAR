namespace INVApp.ContentViews;

public partial class ConfirmBanner : ContentView
{
	public ConfirmBanner()
	{
		InitializeComponent();
	}

    public async void Show(string message)
    {
        ConfirmLabel.Text = message;
        ConfirmFrame.HeightRequest = 60;
        ConfirmFrame.IsVisible = true;

        await Task.Delay(5000);

        ConfirmFrame.IsVisible = false;
    }

    private void HideConfirm()
    {
        ConfirmFrame.HeightRequest = 0;
        ConfirmFrame.IsVisible = false;
    }

    private void CloseConfirmClicked(object sender, EventArgs e)
    {
        HideConfirm();
    }
}