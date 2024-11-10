namespace com.kizwiz.sipnsign.Pages;

public partial class HowToPlayPage : ContentPage
{
	public HowToPlayPage()
	{
		InitializeComponent();
	}

    private async void OnStartPlaying(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new GamePage());
    }
}