using INVApp.ViewModels;
using System.Timers;

namespace INVApp.Views;

public partial class HomePage : ContentPage
{

    public string DevicePlatform => DeviceInfo.Current.Idiom == DeviceIdiom.Desktop ? "Desktop" : "Mobile";
    private System.Timers.Timer _timer;

    public HomePage()
	{
		InitializeComponent();
		BindingContext = new HomePageViewModel(App.DatabaseService);

        StartClock();
    }

    private void OnCarouselItemClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var carouselItem = button.BindingContext as HomePageViewModel.CarouselItem;
        carouselItem?.OnClick?.Invoke(carouselItem.Title);
    }

    private void StartClock()
    {
        _timer = new System.Timers.Timer(1000); 
        _timer.Elapsed += UpdateDateTime;
        _timer.Start();
    }

    private void UpdateDateTime(object sender, ElapsedEventArgs e)
    {
        var now = DateTime.Now;

        string dayWithOrdinal = GetDayWithOrdinal(now.Day);

        string formattedDate = $"{now:dddd}, {dayWithOrdinal} {now:MMMM} {now:hh:mm tt}";

        MainThread.BeginInvokeOnMainThread(() =>
        {
            DateTimeLabel.Text = formattedDate;
            DateTimeLabelPhone.Text = formattedDate;
        });
    }

    private string GetDayWithOrdinal(int day)
    {
        string suffix = day switch
        {
            1 or 21 or 31 => "st",
            2 or 22 => "nd",
            3 or 23 => "rd",
            _ => "th"
        };
        return $"{day}{suffix}";
    }
}