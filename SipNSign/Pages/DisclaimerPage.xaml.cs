namespace com.kizwiz.sipnsign.Pages;

public partial class DisclaimerPage : ContentPage
{
	public DisclaimerPage()
	{
		InitializeComponent();
	}

    private async void OnUnderstandClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}