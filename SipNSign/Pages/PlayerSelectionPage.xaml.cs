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

        // Debug logging to verify player data
        Debug.WriteLine("=== STARTING GAME WITH PLAYERS ===");
        foreach (var player in _viewModel.Players)
        {
            Debug.WriteLine($"Player: {player.Name}, IsMainPlayer: {player.IsMainPlayer}, Score: {player.Score}");
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
            // Log player information
            LogPlayersInfo(parameters);

            // Get services
            var serviceProvider = Application.Current.Handler.MauiContext.Services.GetService<IServiceProvider>();
            var videoService = serviceProvider.GetRequiredService<IVideoService>();
            var logger = serviceProvider.GetRequiredService<ILoggingService>();
            var progressService = serviceProvider.GetRequiredService<IProgressService>();

            // Create and configure game page
            var gamePage = new GamePage(serviceProvider, videoService, logger, progressService);

            // Set parameters BEFORE setting mode
            gamePage.ViewModel.GameParameters = parameters;
            gamePage.ViewModel.CurrentMode = GameMode.Perform;

            // Navigate to game page
            await Navigation.PushAsync(gamePage);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error starting game: {ex.Message}");
            await DisplayAlert("Error", "Failed to start game", "OK");
        }
    }

    private void LogPlayersInfo(GameParameters parameters)
    {
        Debug.WriteLine($"=== Starting game with {parameters.Players.Count} players ===");
        foreach (var player in parameters.Players)
        {
            Debug.WriteLine($"  - Player: {player.Name}, IsMainPlayer: {player.IsMainPlayer}, Score: {player.Score}");
        }
    }
}