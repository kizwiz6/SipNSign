using com.kizwiz.sipnsign.Services;

namespace com.kizwiz.sipnsign.Pages;

public partial class HowToPlayPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;

    public HowToPlayPage(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        InitializeComponent();
    }

    private void OnStartGameClicked(object sender, EventArgs e)
    {
        var videoService = _serviceProvider.GetRequiredService<IVideoService>();
        var logger = _serviceProvider.GetRequiredService<ILoggingService>();
        var gamePage = new GamePage(videoService, logger);
        Navigation.PushAsync(gamePage);
    }
}