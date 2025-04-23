using com.kizwiz.sipnsign.Enums;
using com.kizwiz.sipnsign.Models;
using com.kizwiz.sipnsign.Services;
using com.kizwiz.sipnsign.ViewModels;
using System.Diagnostics;

namespace com.kizwiz.sipnsign.Pages;

/// <summary>
/// Page for selecting single player or multiplayer modes and configuring players
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
        // Set ModeSelectionLayout to invisible to prevent UI overlap
        ModeSelectionLayout.IsVisible = false;

        // Start game with just the main player
        var gameParameters = new GameParameters
        {
            IsMultiplayer = false,
            Players = new List<Player> { new Player { Name = "You", IsMainPlayer = true } }
        };

        await StartGame(gameParameters);
    }

    private void OnMultiplayerClicked(object sender, EventArgs e)
    {
        // Hide mode selection and show player configuration
        ModeSelectionLayout.IsVisible = false;
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
        try
        {
            // Get services from the application
            var serviceProvider = Application.Current.Handler.MauiContext.Services.GetService<IServiceProvider>();
            var videoService = serviceProvider.GetRequiredService<IVideoService>();
            var logger = serviceProvider.GetRequiredService<ILoggingService>();
            var progressService = serviceProvider.GetRequiredService<IProgressService>();

            // Create and configure the game page
            var gamePage = new GamePage(serviceProvider, videoService, logger, progressService);
            gamePage.ViewModel.CurrentMode = GameMode.Perform;
            gamePage.ViewModel.GameParameters = parameters;

            // Navigate to the game page
            await Navigation.PushAsync(gamePage);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error starting game: {ex.Message}");
            await DisplayAlert("Error", "Failed to start game", "OK");
        }
    }
}