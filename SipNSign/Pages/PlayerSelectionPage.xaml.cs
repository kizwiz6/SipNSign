using com.kizwiz.sipnsign.Enums;
using com.kizwiz.sipnsign.Models;
using com.kizwiz.sipnsign.Services;
using com.kizwiz.sipnsign.ViewModels;
using System.Diagnostics;

namespace com.kizwiz.sipnsign.Pages;

/// <summary>
/// 
/// </summary>
public partial class PlayerSelectionPage : ContentPage
{
    private PlayerSelectionViewModel _viewModel;

    public PlayerSelectionPage()
    {
        InitializeComponent();
        _viewModel = new PlayerSelectionViewModel();
        BindingContext = _viewModel;
    }

    private async void OnSinglePlayerClicked(object sender, EventArgs e)
    {
        // Start game with just the main player
        var gameParameters = new GameParameters
        {
            IsMultiplayer = false,
            Players = new List<Player> { new Player { Name = "You", IsMainPlayer = true } }
        };

        await StartGame(gameParameters);
    }

    private async void OnMultiplayerClicked(object sender, EventArgs e)
    {
        // Show player configuration UI
        PlayerConfigLayout.IsVisible = true;
    }

    private async void OnStartGameClicked(object sender, EventArgs e)
    {
        if (_viewModel.Players.Count < 2)
        {
            await DisplayAlert("Not enough players", "Please add at least one additional player", "OK");
            return;
        }

        var gameParameters = new GameParameters
        {
            IsMultiplayer = true,
            Players = _viewModel.Players.ToList()
        };

        await StartGame(gameParameters);
    }

    private async Task StartGame(GameParameters parameters)
    {
        var gameService = Application.Current.MainPage.Handler.MauiContext.Services.GetService<IServiceProvider>();
        var videoService = gameService.GetRequiredService<IVideoService>();
        var logger = gameService.GetRequiredService<ILoggingService>();
        var progressService = gameService.GetRequiredService<IProgressService>();

        var gamePage = new GamePage(gameService, videoService, logger, progressService);
        gamePage.ViewModel.CurrentMode = GameMode.Perform;
        gamePage.ViewModel.GameParameters = parameters;

        await Navigation.PushAsync(gamePage);
    }
}