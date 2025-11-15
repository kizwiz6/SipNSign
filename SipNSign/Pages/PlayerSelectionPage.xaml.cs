using com.kizwiz.sipnsign.Enums;
using com.kizwiz.sipnsign.Models;
using com.kizwiz.sipnsign.Services;
using com.kizwiz.sipnsign.ViewModels;
using System.Diagnostics;

namespace com.kizwiz.sipnsign.Pages;

public partial class PlayerSelectionPage : ContentPage
{
    private PlayerSelectionViewModel _viewModel;
    private bool _isInitialized = false;
    private GameMode _selectedMode; // Store which mode was selected from MainMenu

    public PlayerSelectionPage()
    {
        InitializeComponent();
        _viewModel = new PlayerSelectionViewModel();
        BindingContext = _viewModel;

        // Get the mode that was selected on MainMenuPage
        string modeStr = Preferences.Get("selected_game_mode", "Perform");
        _selectedMode = modeStr == "Guess" ? GameMode.Guess : GameMode.Perform;

        Debug.WriteLine($"PlayerSelectionPage initialized with mode: {_selectedMode}");

        LoadSavedSettings();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_isInitialized)
        {
            ResetView();
        }
        _isInitialized = true;
    }

    private void LoadSavedSettings()
    {
        // Load questions count based on the selected mode
        int savedQuestions = _selectedMode == GameMode.Guess
            ? Preferences.Get(Constants.GUESS_MODE_QUESTIONS_KEY, Constants.DEFAULT_QUESTIONS)
            : Preferences.Get(Constants.PERFORM_MODE_QUESTIONS_KEY, Constants.DEFAULT_PERFORM_QUESTIONS);

        PerformQuestionsSlider.Value = savedQuestions;
        PerformQuestionsValueLabel.Text = $"{savedQuestions} questions";
    }

    private void ResetView()
    {
        ModeSelectionLayout.IsVisible = true;
        PlayerConfigLayout.IsVisible = false;

        _viewModel = new PlayerSelectionViewModel();
        BindingContext = _viewModel;

        LoadSavedSettings();
    }

    // FIXED: No more popup, just use the mode from MainMenu
    private async void OnSinglePlayerClicked(object sender, EventArgs e)
    {
        ModeSelectionLayout.IsVisible = false;

        // Use the mode that was selected on MainMenuPage (no popup)
        int questionsCount = _selectedMode == GameMode.Guess
            ? Preferences.Get(Constants.GUESS_MODE_QUESTIONS_KEY, Constants.DEFAULT_QUESTIONS)
            : Preferences.Get(Constants.PERFORM_MODE_QUESTIONS_KEY, Constants.DEFAULT_PERFORM_QUESTIONS);

        var gameParameters = new GameParameters
        {
            IsMultiplayer = false,
            Players = new List<Player> { new Player { Name = "You", IsMainPlayer = true } },
            QuestionsCount = questionsCount
        };

        await StartGame(gameParameters, _selectedMode);
    }

    // FIXED: No more popup, just show player config
    private void OnMultiplayerClicked(object sender, EventArgs e)
    {
        // Just show player configuration - mode is already set (no popup)
        ModeSelectionLayout.IsVisible = false;
        PlayerConfigLayout.IsVisible = true;
    }

    private async void OnStartGameClicked(object sender, EventArgs e)
    {
        var validationResult = ValidatePlayerNames();
        if (!validationResult.IsValid)
        {
            await DisplayAlert("Invalid Names", validationResult.ErrorMessage, "OK");
            return;
        }

        var allPlayers = _viewModel.GetAllPlayers();

        if (allPlayers.Count < 2)
        {
            await DisplayAlert("Not enough players", "Please add at least one additional player", "OK");
            return;
        }

        int questionsCount = (int)PerformQuestionsSlider.Value;

        Debug.WriteLine($"=== STARTING MULTIPLAYER {_selectedMode} WITH PLAYERS ===");
        foreach (var player in allPlayers)
        {
            Debug.WriteLine($"Player: '{player.Name}', IsMainPlayer: {player.IsMainPlayer}, Score: {player.Score}");
        }
        Debug.WriteLine($"Questions Count: {questionsCount}");

        var gameParameters = new GameParameters
        {
            IsMultiplayer = true,
            Players = allPlayers,
            QuestionsCount = questionsCount
        };

        await StartGame(gameParameters, _selectedMode);
    }

    private void OnPerformQuestionsCountChanged(object sender, ValueChangedEventArgs e)
    {
        int questions = (int)e.NewValue;

        // Save to the appropriate preference based on mode
        if (_selectedMode == GameMode.Guess)
        {
            Preferences.Set(Constants.GUESS_MODE_QUESTIONS_KEY, questions);
        }
        else
        {
            Preferences.Set(Constants.PERFORM_MODE_QUESTIONS_KEY, questions);
        }

        PerformQuestionsValueLabel.Text = $"{questions} questions";
        Debug.WriteLine($"{_selectedMode} mode questions count changed to: {questions}");
    }

    private (bool IsValid, string ErrorMessage) ValidatePlayerNames()
    {
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

    private bool IsValidPlayerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return name.All(c => char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '_');
    }

    private async Task StartGame(GameParameters parameters, GameMode mode)
    {
        try
        {
            LogPlayersInfo(parameters);

            var serviceProvider = Application.Current.Handler.MauiContext.Services.GetService<IServiceProvider>();
            var videoService = serviceProvider.GetRequiredService<IVideoService>();
            var logger = serviceProvider.GetRequiredService<ILoggingService>();
            var progressService = serviceProvider.GetRequiredService<IProgressService>();

            var gamePage = new GamePage(serviceProvider, videoService, logger, progressService);

            // CRITICAL: Set these in the right order
            gamePage.ViewModel.GameParameters = parameters;
            gamePage.ViewModel.CurrentMode = mode;

            Debug.WriteLine($"Starting game: Mode={mode}, IsMultiplayer={parameters.IsMultiplayer}, Players={parameters.Players.Count}");

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
        Debug.WriteLine($"Questions Count: {parameters.QuestionsCount}");
        foreach (var player in parameters.Players)
        {
            Debug.WriteLine($"  - Player: {player.Name}, IsMainPlayer: {player.IsMainPlayer}, Score: {player.Score}");
        }
    }
}