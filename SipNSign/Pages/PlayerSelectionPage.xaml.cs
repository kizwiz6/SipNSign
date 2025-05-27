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
    private bool _isInitialized = false;

    public PlayerSelectionPage()
    {
        InitializeComponent();
        _viewModel = new PlayerSelectionViewModel();
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Reset the view to show mode selection when returning to this page
        if (_isInitialized)
        {
            ResetView();
        }
        _isInitialized = true;
    }

    private void ResetView()
    {
        // Show the mode selection and hide player configuration
        ModeSelectionLayout.IsVisible = true;
        PlayerConfigLayout.IsVisible = false;

        // Reset any other state as needed
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
        // Validate player names first
        var validationResult = ValidatePlayerNames();
        if (!validationResult.IsValid)
        {
            await DisplayAlert("Invalid Names", validationResult.ErrorMessage, "OK");
            return;
        }

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

    private (bool IsValid, string ErrorMessage) ValidatePlayerNames()
    {
        // Check main player name
        var mainPlayerName = _viewModel.MainPlayerName?.Trim();
        if (string.IsNullOrWhiteSpace(mainPlayerName))
        {
            return (false, "Main player name cannot be empty.");
        }

        if (mainPlayerName.Length > 15)
        {
            return (false, "Main player name must be 15 characters or less.");
        }

        if (!IsValidPlayerName(mainPlayerName))
        {
            return (false, "Main player name can only contain letters, numbers, spaces, hyphens, and underscores.");
        }

        // Check additional player names
        var playerNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        playerNames.Add(mainPlayerName);

        foreach (var player in _viewModel.AdditionalPlayers)
        {
            var playerName = player.Name?.Trim();

            if (string.IsNullOrWhiteSpace(playerName))
            {
                return (false, "All player names must be filled in. Remove empty players or add valid names.");
            }

            if (playerName.Length > 15)
            {
                return (false, $"Player name '{playerName}' is too long. Names must be 15 characters or less.");
            }

            if (!IsValidPlayerName(playerName))
            {
                return (false, $"Player name '{playerName}' contains invalid characters. Only letters, numbers, spaces, hyphens, and underscores are allowed.");
            }

            if (playerNames.Contains(playerName))
            {
                return (false, $"Duplicate player name '{playerName}'. All player names must be unique.");
            }

            playerNames.Add(playerName);
        }

        return (true, string.Empty);
    }

    // Helper method to validate individual player names
    private bool IsValidPlayerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        // Allow letters, numbers, spaces, hyphens, and underscores
        return name.All(c => char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '_');
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

            // Navigate to game page using NavigateAsync to properly handle the back stack
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