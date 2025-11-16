using com.kizwiz.sipnsign.Enums;
using com.kizwiz.sipnsign.Models;
using com.kizwiz.sipnsign.Pages;
using com.kizwiz.sipnsign.Services;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace com.kizwiz.sipnsign.ViewModels
{
    /// <summary>
    /// Manages the game state and logic for both Guess and Perform modes
    /// </summary>
    public partial class GameViewModel : INotifyPropertyChanged
    {
        #region Color Constants
        private readonly Color _guessPrimaryColor = Color.FromArgb("#007BFF");
        private readonly Color _performPrimaryColor = Color.FromArgb("#28a745");
        private readonly Color _successColor = Color.FromArgb("#28a745");
        private readonly Color _errorColor = Color.FromArgb("#dc3545");
        #endregion

        #region Private Fields
        private IDispatcherTimer? _timer;
        private readonly IVideoService _videoService;
        private readonly ILoggingService _logger;
        private readonly IProgressService _progressService;
        private readonly IServiceProvider _serviceProvider;
        private const int QuestionTimeLimit = 10;
        private const string TAG = "SipNSignApp";
        private int _remainingTime;
        private bool _isLoading;
        private bool _isProcessingAnswer;
        private int _currentScore;
        private int _correctInARow;
        private SignModel? _currentSign;
        private List<SignModel> _signs;
        private List<int> _availableIndices;
        private string _feedbackText = string.Empty;
        private bool _isGameOver;
        private bool _isGameActive = true;
        private double _progressPercentage;
        private Color _feedbackBackgroundColor;
        private string _guessResults;
        private bool _isFeedbackVisible;
        private bool _debugCommandsWorking = false;
        private bool _isScoreboardVisible = false;
        private int _finalScore;
        private Color _button1Color = Colors.Transparent;
        private Color _button2Color = Colors.Transparent;
        private Color _button3Color = Colors.Transparent;
        private Color _button4Color = Colors.Transparent;
        private GameMode _currentMode = GameMode.Guess;
        private bool _isSignHidden = true;
        private List<double> _answerTimes = new List<double>();
        private double _averageAnswerTime;
        private UserProgress? _userProgress;
        private ICommand _playAgainCommand;
        private ICommand _incorrectPerformCommand;
        private ICommand _correctPerformCommand;
        private ICommand _nextSignCommand;
        private ICommand _confirmResultsCommand;
        private ICommand _recordPlayerAnswerCommand;
        private ICommand _showScoreboardCommand;
        private ICommand _confirmGuessAnswersCommand;
        private string _currentPlayerTurnText = string.Empty;
        private int GetTotalQuestions() => Preferences.Get(Constants.GUESS_MODE_QUESTIONS_KEY, Constants.DEFAULT_QUESTIONS);
        public int TotalQuestions => GetTotalQuestions();
        private int _signsPlayed = 0;
        private int currentAnswer;
        public int CurrentAnswer
        {
            get => currentAnswer;
            set
            {
                if (currentAnswer != value)
                {
                    currentAnswer = value;
                    OnPropertyChanged(nameof(CurrentAnswer));
                }
            }
        }
        #endregion

        /// <summary>
        /// Gets or sets whether the game is currently processing a user's answer
        /// </summary>
        /// <remarks>
        /// Used to prevent multiple rapid inputs while showing feedback
        /// </remarks>
        public bool IsProcessingAnswer
        {
            get => _isProcessingAnswer;
            set
            {
                _isProcessingAnswer = value;
                OnPropertyChanged(nameof(IsProcessingAnswer));
            }
        }

        public bool IsGameActive
        {
            get => _isGameActive;
            set
            {
                _isGameActive = value;
                OnPropertyChanged(nameof(IsGameActive));
            }
        }

        public string GuessResults
        {
            get => _guessResults;
            set
            {
                _guessResults = value;
                OnPropertyChanged(nameof(GuessResults));
            }
        }

        public int SignsPlayed
        {
            get => _signsPlayed;
            set
            {
                if (_signsPlayed != value)
                {
                    _signsPlayed = value;
                    OnPropertyChanged(nameof(SignsPlayed));
                }
            }
        }

        #region Public Properties
        public Color PrimaryColor => _currentMode == GameMode.Guess ? _guessPrimaryColor : _performPrimaryColor;
        public Color ProgressBarColor => PrimaryColor;
        public Color ButtonBaseColor
        {
            get
            {
                if (Application.Current?.Resources != null &&
                    Application.Current.Resources.TryGetValue("AnswerButton", out var colorValue) &&
                    colorValue is Color themeColor)
                {
                    return themeColor;
                }
                return _guessPrimaryColor;  // Fallback to blue only if theme color not found
            }
        }

        public ICommand ConfirmGuessAnswersCommand
        {
            get
            {
                return _confirmGuessAnswersCommand ??= new Command(async () =>
                {
                    if (IsProcessingAnswer) return;

                    Debug.WriteLine("ConfirmGuessAnswersCommand executed");

                    // In multiplayer ensure everyone answered before proceeding
                    if (IsMultiplayer && !HasAllPlayersAnswered)
                    {
                        var unansweredPlayers = Players.Where(p => !p.HasAnswered).ToList();
                        var playerNames = string.Join(", ", unansweredPlayers.Select(p => p.Name));

                        await Application.Current.MainPage.DisplayAlert(
                            "Waiting for Players",
                            $"Still waiting for: {playerNames}\n\nMake sure all players have selected their answers (1-4).",
                            "OK");
                        return;
                    }

                    try
                    {
                        IsProcessingAnswer = true;

                        // Award scores for all players based on their selected answers (only on confirm)
                        foreach (var player in Players)
                        {
                            if (player.HasAnswered && player.GotCurrentAnswerCorrect)
                            {
                                player.Score += 1;
                            }
                        }

                        // Keep main player's CurrentScore in sync
                        var main = Players.FirstOrDefault(p => p.IsMainPlayer);
                        if (main != null)
                        {
                            CurrentScore = main.Score;
                            // Log only main player's activity as before
                            if (main.HasAnswered)
                            {
                                await LogGameActivity(main.GotCurrentAnswerCorrect);
                            }
                        }

                        // Show results (this already displays overlays/alerts)
                        await ShowGuessResults();

                        // Check for achievements / perfect round
                        CheckForPerfectRound();

                        SignsPlayed++;

                        // Small pause so UI can update briefly (shorter than before)
                        await Task.Delay(300);

                        // Continue to next sign (or end)
                        if (_availableIndices.Count > 0)
                        {
                            LoadNextSign();
                        }
                        else
                        {
                            EndGame();
                        }

                        // Force UI update for Players and HasAllPlayersAnswered (now changed)
                        OnPropertyChanged(nameof(Players));
                        OnPropertyChanged(nameof(HasAllPlayersAnswered));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error in ConfirmGuessAnswersCommand: {ex.Message}");
                        await Application.Current.MainPage.DisplayAlert("Error",
                            "There was an error processing answers. Please try again.", "OK");
                    }
                    finally
                    {
                        IsProcessingAnswer = false;
                    }
                });
            }
        }
        /// <summary>
        /// Event triggered when a sign reveal is requested.
        /// </summary>
        public event EventHandler? SignRevealRequested;
        public Color FeedbackSuccessColor => _successColor.WithAlpha(0.9f);
        public Color FeedbackErrorColor => _errorColor.WithAlpha(0.9f);
        public string ModeTitle
        {
            get
            {
                if (_currentMode == GameMode.Guess)
                    return IsMultiplayer ? "Guess Mode - Multiplayer" : "Guess Mode - Singular";
                return IsMultiplayer ? "Perform Mode - Multiplayer" : "Perform Mode - Singular";
            }
        }
        public bool IsTimerEnabled => Preferences.Get(Constants.TIMER_DURATION_KEY, Constants.DEFAULT_TIMER_DURATION) > 0;

        // Property to determine if all players have answered in Multiplayer mode
        public bool HasAllPlayersAnswered
        {
            get
            {
                if (!IsMultiplayer || Players == null || !Players.Any())
                    return true;

                // In Perform mode players mark HasAnswered (via ✓/✗) — check that.
                if (IsPerformMode)
                {
                    bool allAnswered = Players.All(p => p.HasAnswered);
                    Debug.WriteLine($"HasAllPlayersAnswered (Perform): {allAnswered} ({Players.Count(p => p.HasAnswered)}/{Players.Count})");
                    return allAnswered;
                }

                // In Guess mode players choose a numbered answer (SelectedAnswer != 0)
                bool allAnsweredGuess = Players.All(p => p.SelectedAnswer != 0);
                Debug.WriteLine($"HasAllPlayersAnswered (Guess): {allAnsweredGuess} ({Players.Count(p => p.SelectedAnswer != 0)}/{Players.Count})");
                return allAnsweredGuess;
            }
        }

        // Helper method to directly update a player and force UI refresh
        private void UpdatePlayerStatus(Player player, bool isCorrect)
        {
            if (player == null) return;

            // Update the player properties
            player.GotCurrentAnswerCorrect = true; // Mark as answered

            if (isCorrect)
            {
                player.Score++;
            }

            // Force UI refresh
            OnPropertyChanged(nameof(Players));
            OnPropertyChanged(nameof(HasAllPlayersAnswered));
        }

        // Property to control scoreboard visibility in Multiplayer mode
        public bool IsScoreboardVisible
        {
            get => _isScoreboardVisible;
            set
            {
                _isScoreboardVisible = value;
                OnPropertyChanged(nameof(IsScoreboardVisible));
            }
        }

        public GameMode CurrentMode
        {
            get => _currentMode;
            set
            {
                if (_currentMode != value)
                {
                    Debug.WriteLine($"Mode changing from {_currentMode} to {value}");
                    _currentMode = value;
                    Debug.WriteLine($"IsPerformMode: {IsPerformMode}");
                    Debug.WriteLine($"IsGuessMode: {IsGuessMode}");

                    OnPropertyChanged(nameof(CurrentMode));
                    OnPropertyChanged(nameof(IsGuessMode));
                    OnPropertyChanged(nameof(IsPerformMode));
                    OnPropertyChanged(nameof(ModeTitle));
                    OnPropertyChanged(nameof(HasAllPlayersAnswered)); // ensure button state refreshes when mode changes
                }
            }
        }

        public bool IsGuessMode => CurrentMode == GameMode.Guess;
        public bool IsPerformMode => CurrentMode == GameMode.Perform;

        public int RemainingTime
        {
            get => _remainingTime;
            set
            {
                if (_remainingTime != value)
                {
                    _remainingTime = value;
                    OnPropertyChanged(nameof(RemainingTime));
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        public int CurrentScore
        {
            get => _currentScore;
            set
            {
                if (_currentScore != value)
                {
                    _currentScore = value;
                    OnPropertyChanged(nameof(CurrentScore));
                    ProgressPercentage = _signs.Count > 0 ? (double)value / _signs.Count : 0;
                }
            }
        }

        public SignModel? CurrentSign
        {
            get => _currentSign;
            private set
            {
                if (_currentSign != value)
                {
                    _currentSign = value;
                    ResetButtonColors();  // Reset to theme colors when new sign is loaded
                    OnPropertyChanged(nameof(CurrentSign));
                }
            }
        }

        public bool IsGameOver
        {
            get => _isGameOver;
            set
            {
                _isGameOver = value;
                OnPropertyChanged(nameof(IsGameOver));
            }
        }

        public double ProgressPercentage
        {
            get => _progressPercentage;
            set
            {
                if (_progressPercentage != value)
                {
                    _progressPercentage = value;
                    OnPropertyChanged(nameof(ProgressPercentage));
                }
            }
        }
        public Color FeedbackBackgroundColor
        {
            get => _feedbackBackgroundColor;
            set
            {
                if (_feedbackBackgroundColor != value)
                {
                    _feedbackBackgroundColor = value;
                    OnPropertyChanged(nameof(FeedbackBackgroundColor));
                }
            }
        }

        public bool IsFeedbackVisible
        {
            get => _isFeedbackVisible && Preferences.Get(Constants.SHOW_FEEDBACK_KEY, true);
            set
            {
                if (_isFeedbackVisible != value)
                {
                    _isFeedbackVisible = value;
                    OnPropertyChanged(nameof(IsFeedbackVisible));
                }
            }
        }

        public string FeedbackText
        {
            get => _feedbackText;
            set
            {
                if (_feedbackText != value)
                {
                    _feedbackText = value;
                    OnPropertyChanged(nameof(FeedbackText));
                }
            }
        }

        public int FinalScore
        {
            get => _finalScore;
            set
            {
                if (_finalScore != value)
                {
                    _finalScore = value;
                    OnPropertyChanged(nameof(FinalScore));
                }
            }
        }

        public Color Button1Color
        {
            get => _button1Color;
            set
            {
                _button1Color = value;
                OnPropertyChanged(nameof(Button1Color));
            }
        }

        public Color Button2Color
        {
            get => _button2Color;
            set
            {
                _button2Color = value;
                OnPropertyChanged(nameof(Button2Color));
            }
        }

        public Color Button3Color
        {
            get => _button3Color;
            set
            {
                _button3Color = value;
                OnPropertyChanged(nameof(Button3Color));
            }
        }

        public Color Button4Color
        {
            get => _button4Color;
            set
            {
                _button4Color = value;
                OnPropertyChanged(nameof(Button4Color));
            }
        }

        /// <summary>
        /// Determines the color for a button based on the current state of the answer selection.
        /// Displays feedback (green/red) when an answer is being processed, otherwise uses the theme color.
        /// </summary>
        /// <param name="buttonIndex">The index of the button being checked for color.</param>
        /// <param name="selectedIndex">The index of the button that was selected by the user.</param>
        /// <param name="correctIndex">The index of the button that is the correct answer.</param>
        /// <param name="isCorrect">A boolean indicating whether the selected answer is correct or not.</param>
        /// <returns>The color to be applied to the button based on the selection state.</returns>
        private Color GetButtonColor(int buttonIndex, int selectedIndex, int correctIndex, bool isCorrect)
        {
            if (IsProcessingAnswer)
            {
                if (buttonIndex == selectedIndex)
                {
                    return isCorrect ? _successColor : _errorColor;
                }
                else if (buttonIndex == correctIndex && !isCorrect)
                {
                    return _successColor;
                }
                else
                {
                    if (Application.Current?.Resources.TryGetValue("AnswerButton", out var themeColorObject) == true &&
                        themeColorObject is Color themeColor)
                    {
                        return themeColor;
                    }
                    return ButtonBaseColor;
                }
            }

            // Non-processing state - use theme color
            if (Application.Current?.Resources.TryGetValue("AnswerButton", out var defaultThemeColor) == true &&
                defaultThemeColor is Color color)
            {
                return color;
            }
            return ButtonBaseColor;
        }

        public bool IsSignHidden
        {
            get => _isSignHidden;
            set
            {
                if (_isSignHidden != value)
                {
                    _isSignHidden = value;
                    OnPropertyChanged(nameof(IsSignHidden));
                    OnPropertyChanged(nameof(IsSignRevealed));
                }
            }
        }

        public bool IsSignRevealed => !IsSignHidden;

        // Multiplayer Properties
        public bool IsMultiplayer => GameParameters?.IsMultiplayer ?? false;

        public ObservableCollection<Player> Players { get; } = new ObservableCollection<Player>();

        public string CurrentPlayerTurnText
        {
            get => _currentPlayerTurnText;
            set
            {
                if (_currentPlayerTurnText != value)
                {
                    _currentPlayerTurnText = value;
                    OnPropertyChanged(nameof(CurrentPlayerTurnText));
                }
            }
        }

        #endregion

        #region Commands
        public required ICommand AnswerCommand { get; set; }
        public ICommand PlayAgainCommand
        {
            get
            {
                return _playAgainCommand ??= new Command(() =>
                {
                    Debug.WriteLine("PlayAgainCommand executed");
                    IsFeedbackVisible = false;
                    IsGameOver = false;
                    IsGameActive = true;
                    ResetButtonColors();  // Reset any colored buttons
                    ResetGame();
                    LoadNextSign();

                    // Notify all relevant property changes
                    OnPropertyChanged(nameof(IsGameOver));
                    OnPropertyChanged(nameof(IsGameActive));
                    OnPropertyChanged(nameof(IsFeedbackVisible));
                });
            }
        }
        public ICommand VideoLoadedCommand { get; private set; }
        public required ICommand RevealSignCommand { get; set; }
        public ICommand CorrectPerformCommand => _correctPerformCommand;
        public ICommand IncorrectPerformCommand => _incorrectPerformCommand;
        public ICommand SwitchModeCommand { get; private set; }
        public ICommand NextSignCommand
        {
            get
            {
                return _nextSignCommand ??= new Command(async () =>
                {
                    if (IsProcessingAnswer) return;

                    // In multiplayer mode, check if all players have answered
                    if (IsMultiplayer && !HasAllPlayersAnswered)
                    {
                        var unansweredPlayers = Players.Where(p => !p.HasAnswered).ToList();
                        var playerNames = string.Join(", ", unansweredPlayers.Select(p => p.Name));

                        await Application.Current.MainPage.DisplayAlert(
                            "Waiting for Players",
                            $"Still waiting for: {playerNames}\n\nMake sure all players have recorded their answers before moving to the next sign.",
                            "OK");
                        return;
                    }

                    try
                    {
                        IsProcessingAnswer = true;
                        IsFeedbackVisible = false;

                        // Increment signs played counter
                        SignsPlayed++;

                        // In Perform Mode, hide the sign before loading next
                        if (IsPerformMode)
                        {
                            IsSignHidden = true;
                        }

                        Debug.WriteLine("NextSignCommand: All players answered, proceeding to next sign");

                        // Check if we have more signs
                        if (_availableIndices.Count > 0)
                        {
                            LoadNextSign();
                            Debug.WriteLine($"NextSignCommand: Loaded next sign. Remaining signs: {_availableIndices.Count}");
                        }
                        else
                        {
                            Debug.WriteLine("NextSignCommand: No more signs, ending game");
                            EndGame();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error in NextSignCommand: {ex.Message}");
                        await Application.Current.MainPage.DisplayAlert("Error",
                            "There was an error loading the next sign. Please try again.", "OK");
                    }
                    finally
                    {
                        IsProcessingAnswer = false;
                    }
                });
            }
        }
        public ICommand RecordPlayerAnswerCommand
        {
            get
            {
                return _recordPlayerAnswerCommand ??= new Command<object>(param =>
                {
                    Debug.WriteLine($"RecordPlayerAnswerCommand called with parameter type: {param?.GetType().Name ?? "null"}");

                    // Extract player and correctness value
                    Player player = null;
                    bool isCorrect = false;

                    if (param is PlayerAnswerParameter playerParam)
                    {
                        player = playerParam.Player;
                        isCorrect = playerParam.IsCorrect;
                        Debug.WriteLine($"Got PlayerAnswerParameter: Player={player?.Name}, IsCorrect={isCorrect}");
                    }
                    else
                    {
                        Debug.WriteLine("Invalid parameter type - expected PlayerAnswerParameter");
                        return;
                    }

                    if (player == null)
                    {
                        Debug.WriteLine("Player is null - can't record answer");
                        return;
                    }

                    // Update player's answer status
                    player.GotCurrentAnswerCorrect = isCorrect;
                    Debug.WriteLine($"Set {player.Name}.GotCurrentAnswerCorrect = {isCorrect}");

                    // Update score if correct
                    if (isCorrect)
                    {
                        player.Score += 1;
                        Debug.WriteLine($"Updated {player.Name} score to {player.Score}");
                    }

                    // Update UI on main thread
                    MainThread.BeginInvokeOnMainThread(() => {
                        // Force UI refresh
                        OnPropertyChanged(nameof(Players));
                        OnPropertyChanged(nameof(HasAllPlayersAnswered));

                        // Show feedback
                        FeedbackText = $"{player.Name} {(isCorrect ? "got it right! ✓" : "got it wrong ✗")}";
                        FeedbackBackgroundColor = GetFeedbackColor(isCorrect);
                        IsFeedbackVisible = true;

                        // Auto-hide feedback after delay
                        Device.StartTimer(TimeSpan.FromSeconds(2), () => {
                            IsFeedbackVisible = false;
                            return false; // Don't repeat
                        });
                    });
                });
            }
        }
        #endregion

        #region Events
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="GameViewModel"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service responsible for service-related functionality.</param>
        /// <param name="videoService">The service responsible for video-related functionality.</param>
        /// <param name="logger">The service responsible for logging.</param>
        /// <param name="progressService">The service responsible for managing progress indicators.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any of the required services are null.
        /// </exception>
        public GameViewModel(IServiceProvider serviceProvider, IVideoService videoService, ILoggingService logger, IProgressService progressService)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _videoService = videoService ?? throw new ArgumentNullException(nameof(videoService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));

            // Set default to colored feedback (false means colored)
            if (!Preferences.ContainsKey(Constants.TRANSPARENT_FEEDBACK_KEY))
            {
                Preferences.Set(Constants.TRANSPARENT_FEEDBACK_KEY, false);
            }

            // Initialize signs list first
            _signs = new SignRepository().GetSigns();
            _availableIndices = new List<int>();
            _feedbackText = string.Empty;
            _feedbackBackgroundColor = Colors.Transparent;
            _guessResults = string.Empty;

            // Initialize SignRevealRequested with a default handler
            SignRevealRequested = (sender, args) => { };

            // Initialize commands
            AnswerCommand = new Command<string>(HandleAnswer);
            VideoLoadedCommand = new Command(() => IsLoading = false);
            RevealSignCommand = new Command(RevealSign);
            CurrentVideoSource = MediaSource.FromFile("again.mp4");
            SwitchModeCommand = new Command<GameMode>(SwitchMode);

            InitializeCommands();
            InitializePlayers();

            // Initialize videos in a fire-and-forget Task
            Task.Run(async () =>
            {
                try
                {
                    await _videoService.InitializeVideos();
                    MainThread.BeginInvokeOnMainThread(() => ResetGame());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"GameViewModel initialization error: {ex}");
                }
            });
        }
        #endregion

        #region Private Methods
        private void InitializeCommands()
        {
            _playAgainCommand = new Command(ResetGame);
            VideoLoadedCommand = new Command(() => IsLoading = false);

            _correctPerformCommand = new Command(async () =>
            {
                if (IsProcessingAnswer) return;
                try
                {
                    IsProcessingAnswer = true;
                    _timer?.Stop();
                    CurrentScore++;

                    if (Preferences.Get(Constants.SHOW_FEEDBACK_KEY, true))
                    {
                        FeedbackText = GetFeedbackText(true);
                        FeedbackBackgroundColor = GetFeedbackColor(true);
                        IsFeedbackVisible = true;
                    }

                    await Task.Delay(2000);
                    IsFeedbackVisible = false;
                    IsSignHidden = true;
                    await LogGameActivity(true);
                    LoadNextSign();
                }
                finally
                {
                    IsProcessingAnswer = false;
                }
            });

            _incorrectPerformCommand = new Command(async () =>
            {
                if (IsProcessingAnswer) return;
                try
                {
                    IsProcessingAnswer = true;
                    _timer?.Stop();

                    if (Preferences.Get(Constants.SHOW_FEEDBACK_KEY, true))
                    {
                        FeedbackText = GetFeedbackText(false);
                        FeedbackBackgroundColor = GetFeedbackColor(false);
                        IsFeedbackVisible = true;
                    }

                    await Task.Delay(2000);
                    IsFeedbackVisible = false;
                    IsSignHidden = true;
                    await LogGameActivity(false);
                    LoadNextSign();
                }
                finally
                {
                    IsProcessingAnswer = false;
                }
            });

            SwitchModeCommand = new Command<GameMode>(SwitchMode);
        }

        private string GetFeedbackText(bool isCorrect)
        {
            bool isSoberMode = Preferences.Get(Constants.SOBER_MODE_KEY, false);

            if (isCorrect)
            {
                return $"Correct!\n\nThe sign means '{CurrentSign?.CorrectAnswer}'.";
            }
            else
            {
                return isSoberMode
                    ? $"Incorrect.\n\nThe sign means '{CurrentSign?.CorrectAnswer}'.\n\nKeep Learning!"
                    : $"Incorrect.\n\nThe sign means '{CurrentSign?.CorrectAnswer}'.\n\nTake a sip!";
            }
        }

        private void InitializeGame()
        {
            ProgressPercentage = 0;
            IsFeedbackVisible = false;
            FeedbackBackgroundColor = Colors.Transparent;
            ResetGame();
        }

        private async void Timer_Tick(object? sender, EventArgs e)
        {
            if (RemainingTime > 0)
            {
                RemainingTime--;
            }
            else
            {
                _timer?.Stop();
                await Task.Run(HandleTimeOut);  // Run on background thread
            }
        }

        /// <summary>
        /// Handles when the timer runs out in Guess Mode
        /// </summary>
        private async void HandleTimeOut()
        {
            if (IsProcessingAnswer) return;

            try
            {
                IsProcessingAnswer = true;

                if (Preferences.Get(Constants.SHOW_FEEDBACK_KEY, true))
                {
                    FeedbackBackgroundColor = GetFeedbackColor(false);
                    FeedbackText = Preferences.Get(Constants.SOBER_MODE_KEY, false)
                        ? $"Time's up!\n\nThe sign means '{CurrentSign?.CorrectAnswer}'.\n\nKeep practicing!"
                        : $"Time's up!\n\nThe sign means '{CurrentSign?.CorrectAnswer}'.\n\nTake a sip!";
                    IsFeedbackVisible = true;
                }

                // Wait for feedback display duration
                await Task.Delay(Preferences.Get(Constants.INCORRECT_DELAY_KEY, Constants.DEFAULT_DELAY));

                // Move to next question
                await Task.Delay(Preferences.Get(Constants.INCORRECT_DELAY_KEY, Constants.DEFAULT_DELAY));
                IsFeedbackVisible = false;
                await LogGameActivity(false);
                LoadNextSign();
            }
            finally
            {
                IsProcessingAnswer = false;
            }
        }

        private bool CheckAnswer(string answer)
        {
            return answer == CurrentSign?.CorrectAnswer;
        }

        public async void HandleAnswer(string answer)
        {
            if (!IsGameActive || IsGameOver || IsProcessingAnswer) return;

            try
            {
                IsProcessingAnswer = true;
                if (_timer != null) _timer.Stop();

                double answerTime = QuestionTimeLimit - RemainingTime;
                UpdateAnswerTime(answerTime);

                bool isCorrect = CheckAnswer(answer);

                // Run UI updates on main thread
                await MainThread.InvokeOnMainThreadAsync(() => {
                    UpdateButtonColor(answer, isCorrect);

                    // Only show feedback if enabled
                    if (Preferences.Get(Constants.SHOW_FEEDBACK_KEY, true))
                    {
                        FeedbackText = GetFeedbackText(isCorrect);
                        FeedbackBackgroundColor = GetFeedbackColor(isCorrect);
                        IsFeedbackVisible = true;
                    }
                });

                // Log activity
                if (isCorrect)
                {
                    CurrentScore++;
                    await LogGameActivity(true, answerTime);
                }
                else
                {
                    await LogGameActivity(false, answerTime);
                }

                // Check if this is the last question
                bool isLastQuestion = _availableIndices.Count == 0;

                await ShowFeedbackAndContinue(isCorrect);

                if (isLastQuestion)
                {
                    EndGame();
                }
            }
            finally
            {
                IsProcessingAnswer = false;
            }
        }

        public Color GetFeedbackColor(bool isCorrect)
        {
            bool useTransparent = Preferences.Get(Constants.TRANSPARENT_FEEDBACK_KEY, false);
            Color color = useTransparent ?
                Colors.Black.WithAlpha(0.5f) :
                (isCorrect ? Color.FromArgb("#28a745") : Color.FromArgb("#dc3545"));
            Debug.WriteLine($"Current mode: {CurrentMode}, Transparent: {useTransparent}, IsCorrect: {isCorrect}, Color: {color}");
            return color;
        }

        private void ShowFeedback(bool isCorrect)
        {
            // Only show feedback if enabled in settings
            if (!Preferences.Get(Constants.SHOW_FEEDBACK_KEY, true))
            {
                return;
            }

            FeedbackBackgroundColor = GetFeedbackColor(isCorrect);

            // If in multiplayer, show a different message that doesn't target a specific player
            if (IsMultiplayer)
            {
                bool isSoberMode = Preferences.Get(Constants.SOBER_MODE_KEY, false);
                FeedbackText = isCorrect
                    ? $"Correct!\n\nThe sign means '{CurrentSign?.CorrectAnswer}'."
                    : (isSoberMode
                        ? $"Incorrect.\n\nThe sign means '{CurrentSign?.CorrectAnswer}'.\n\nKeep Learning!"
                        : $"Incorrect.\n\nThe sign means '{CurrentSign?.CorrectAnswer}'.\n\nTake a sip!");
            }
            else
            {
                FeedbackText = GetFeedbackText(isCorrect);
            }

            IsFeedbackVisible = true;
        }

        private void UpdateAnswerTime(double time)
        {
            _answerTimes.Add(time);
            _averageAnswerTime = _answerTimes.Average();
        }

        public void RevealSign()
        {
            _logger.Debug($"RevealSign called. CurrentSign is: {CurrentSign?.CorrectAnswer ?? "null"}");

            // Important: Force UI update before changing IsSignHidden
            MainThread.BeginInvokeOnMainThread(() => {
                // Log debugging information
                Debug.WriteLine($"=== REVEALING SIGN FOR {Players.Count} PLAYERS ===");
                foreach (var p in Players)
                {
                    Debug.WriteLine($"Player: {p.Name}, IsMainPlayer: {p.IsMainPlayer}, Score: {p.Score}");

                    // IMPORTANT: Reset player answer status
                    p.GotCurrentAnswerCorrect = false;
                }

                // Force UI refresh
                OnPropertyChanged(nameof(Players));
                OnPropertyChanged(nameof(IsMultiplayer));
                OnPropertyChanged(nameof(CurrentPlayerTurnText));

                // Now change sign visibility
                IsSignHidden = false;
                OnPropertyChanged(nameof(IsSignRevealed));

                // Request the sign to be revealed
                SignRevealRequested?.Invoke(this, EventArgs.Empty);
            });
        }

        private async Task HandleCorrectAnswer()
        {
            if (CurrentSign == null) return;

            CurrentScore++;
            await _progressService.LogActivityAsync(new ActivityLog
            {
                Id = Guid.NewGuid().ToString(),
                Type = ActivityType.Practice,
                Description = $"Learned sign for '{CurrentSign.CorrectAnswer}'",
                IconName = "practice_icon",
                Timestamp = DateTime.Now,
                Score = CurrentScore.ToString()
            });
        }

        private async void HandleCorrectPerform()
        {
            if (IsProcessingAnswer) return;

            try
            {
                IsProcessingAnswer = true;
                Debug.WriteLine($"HandleCorrectPerform - Current transparency setting: {Preferences.Get(Constants.TRANSPARENT_FEEDBACK_KEY, false)}");

                IsSignHidden = true;
                _timer?.Stop();
                CurrentScore++;
                FeedbackText = "Nice work!\n\nPrepare for your next sign!";
                FeedbackBackgroundColor = GetFeedbackColor(true);
                await ShowFeedbackAndContinue(true);
                await LogGameActivity(true);
            }
            finally
            {
                IsProcessingAnswer = false;
            }
        }

        private async void HandleIncorrectPerform()
        {
            if (IsProcessingAnswer) return;

            try
            {
                IsProcessingAnswer = true;
                Debug.WriteLine($"HandleIncorrectPerform - Current transparency setting: {Preferences.Get(Constants.TRANSPARENT_FEEDBACK_KEY, false)}");

                IsSignHidden = true;
                _timer?.Stop();
                bool isSoberMode = Preferences.Get(Constants.SOBER_MODE_KEY, false);
                FeedbackText = isSoberMode
                    ? $"Keep practicing '{CurrentSign?.CorrectAnswer}'!"
                    : $"Remember to practice '{CurrentSign?.CorrectAnswer}'!\n\nTake a sip!";
                FeedbackBackgroundColor = GetFeedbackColor(false);
                await ShowFeedbackAndContinue(false);
                await LogGameActivity(false);
            }
            finally
            {
                IsProcessingAnswer = false;
            }
        }

        /// <summary>
        /// Shows feedback to user and handles transition to next sign
        /// </summary>
        public async Task ShowFeedbackAndContinue(bool isCorrect)
        {
            Debug.WriteLine("Starting ShowFeedbackAndContinue");
            if (IsGameOver)
            {
                Debug.WriteLine("Game is over, returning early");
                return;
            }

            IsFeedbackVisible = true;
            FeedbackBackgroundColor = GetFeedbackColor(isCorrect);

            int delay = isCorrect ? 2000 : Preferences.Get(Constants.INCORRECT_DELAY_KEY, Constants.DEFAULT_DELAY);
            Debug.WriteLine($"Waiting for {delay}ms");
            await Task.Delay(delay);

            if (!IsGameOver)
            {
                Debug.WriteLine("Processing continue after delay");
                IsFeedbackVisible = false;
                if (IsPerformMode) IsSignHidden = true;

                ResetButtonColors();

                if (IsMultiplayer && IsPerformMode)
                {
                    return;
                }
                else
                {
                    if (_availableIndices.Count > 0)
                    {
                        Debug.WriteLine("Loading next sign");
                        LoadNextSign();
                    }
                    else
                    {
                        Debug.WriteLine("No more questions available");
                        EndGame();
                    }
                }
            }
            Debug.WriteLine("ShowFeedbackAndContinue completed");
        }

        private void UpdateButtonColor(string answer, bool isCorrect)
        {
            int correctAnswerIndex = -1;
            for (int i = 0; i < 4; i++)
            {
                if (CurrentSign?.Choices[i] == CurrentSign?.CorrectAnswer)
                {
                    correctAnswerIndex = i;
                    break;
                }
            }

            int selectedAnswerIndex = -1;
            for (int i = 0; i < 4; i++)
            {
                if (CurrentSign?.Choices[i] == answer)
                {
                    selectedAnswerIndex = i;
                    break;
                }
            }

            Button1Color = GetButtonColor(0, selectedAnswerIndex, correctAnswerIndex, isCorrect);
            Button2Color = GetButtonColor(1, selectedAnswerIndex, correctAnswerIndex, isCorrect);
            Button3Color = GetButtonColor(2, selectedAnswerIndex, correctAnswerIndex, isCorrect);
            Button4Color = GetButtonColor(3, selectedAnswerIndex, correctAnswerIndex, isCorrect);
        }

        private void ResetButtonColors()
        {
            Debug.WriteLine("ResetButtonColors called");

            if (Application.Current?.Resources != null &&
                Application.Current.Resources.TryGetValue("AnswerButton", out var colorValue) &&
                colorValue is Color themeColor)
            {
                Debug.WriteLine($"Found theme color: {themeColor}");
                Button1Color = themeColor;
                Button2Color = themeColor;
                Button3Color = themeColor;
                Button4Color = themeColor;
            }
            else
            {
                Debug.WriteLine("Using fallback ButtonBaseColor");
                Button1Color = ButtonBaseColor;
                Button2Color = ButtonBaseColor;
                Button3Color = ButtonBaseColor;
                Button4Color = ButtonBaseColor;
            }
        }

        private void StartTimer()
        {
            try
            {
                if (_timer == null)
                {
                    Debug.WriteLine("Creating new timer");
                    var dispatcher = Application.Current?.Dispatcher;
                    if (dispatcher == null)
                    {
                        Debug.WriteLine("Dispatcher is null. Cannot create timer.");
                        return;
                    }

                    _timer = dispatcher.CreateTimer();
                    if (_timer == null)
                    {
                        Debug.WriteLine("Failed to create timer.");
                        return;
                    }

                    _timer.Interval = TimeSpan.FromSeconds(1);
                    _timer.Tick += Timer_Tick;
                }

                int duration = Preferences.Get(Constants.TIMER_DURATION_KEY, Constants.DEFAULT_TIMER_DURATION);
                if (duration > 0)
                {
                    RemainingTime = duration;
                    _timer?.Start();
                    Debug.WriteLine($"Timer started with duration: {duration}");
                }
                else
                {
                    RemainingTime = 0;
                    Debug.WriteLine("Timer disabled.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in StartTimer: {ex.Message}");
            }
        }

        private async Task UpdatePracticeTime()
        {
            if (_userProgress == null) return;
            _userProgress.TotalPracticeTime = _userProgress.TotalPracticeTime.Add(TimeSpan.FromMinutes(1));
            await _progressService.SaveProgressAsync(_userProgress);
        }

        private void SwitchMode(GameMode mode)
        {
            CurrentMode = mode;
        }

        private async Task LogGameActivity(bool isCorrect, double? answerTime = null)
        {
            if (_userProgress == null)
            {
                _userProgress = await _progressService.GetUserProgressAsync();
            }

            _userProgress.TotalAttempts++;
            if (isCorrect)
            {
                _userProgress.CorrectAttempts++;

                if (IsGuessMode)
                    _userProgress.GuessModeSigns++;
                else
                    _userProgress.PerformModeSigns++;
            }

            if (isCorrect && answerTime.HasValue && answerTime.Value < 5.0)
            {
                await _progressService.LogActivityAsync(new ActivityLog
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = ActivityType.Practice,
                    IconName = "speed_icon",
                    Timestamp = DateTime.Now
                });
            }

            _userProgress.Accuracy = (double)_userProgress.CorrectAttempts / _userProgress.TotalAttempts;
            await _progressService.SaveProgressAsync(_userProgress);

            await _progressService.LogActivityAsync(new ActivityLog
            {
                Id = Guid.NewGuid().ToString(),
                Type = ActivityType.Practice,
                Description = isCorrect ?
                    $"Correctly signed '{CurrentSign?.CorrectAnswer}'" :
                    $"Incorrectly signed '{CurrentSign?.CorrectAnswer}'",
                IconName = isCorrect ? "quiz_correct_icon" : "quiz_incorrect_icon",
                Timestamp = DateTime.Now,
                Score = isCorrect ? "+1" : "N/A"
            });

            if (isCorrect)
            {
                _correctInARow++;
                if (_correctInARow >= 10)
                {
                    await _progressService.UpdateAchievementsAsync();
                }
            }
            else
            {
                _correctInARow = 0;
            }

            if (_availableIndices.Count == 0 && TotalQuestions == 100 && _averageAnswerTime < 3.0)
            {
                await _progressService.LogActivityAsync(new ActivityLog
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = ActivityType.Practice,
                    Description = "Completed 100-question Guess Mode session with average time under 3 seconds",
                    IconName = "speed_master_icon",
                    Timestamp = DateTime.Now
                });
            }
        }

        // NEW: Log multiplayer game completion with achievements
        private async void LogMultiplayerGameCompletion()
        {
            try
            {
                var mainPlayer = Players.FirstOrDefault(p => p.IsMainPlayer);
                if (mainPlayer == null) return;

                var sortedPlayers = Players.OrderByDescending(p => p.Score).ToList();
                var winner = sortedPlayers.First();
                bool mainPlayerWon = mainPlayer == winner;
                int playerCount = Players.Count;

                await _progressService.LogActivityAsync(new ActivityLog
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = ActivityType.Practice,
                    Description = $"Multiplayer game completed with {playerCount} players",
                    IconName = "multiplayer_icon",
                    Timestamp = DateTime.Now,
                    Score = $"{mainPlayer.Score}/{SignsPlayed}"
                });

                if (mainPlayer.Score == SignsPlayed)
                {
                    await _progressService.LogActivityAsync(new ActivityLog
                    {
                        Id = Guid.NewGuid().ToString(),
                        Type = ActivityType.Practice,
                        Description = "Perfect multiplayer game - got all signs correct!",
                        IconName = "perfect_multi_icon",
                        Timestamp = DateTime.Now
                    });
                }

                if (playerCount >= 5)
                {
                    await _progressService.LogActivityAsync(new ActivityLog
                    {
                        Id = Guid.NewGuid().ToString(),
                        Type = ActivityType.Practice,
                        Description = $"Hosted a large party with {playerCount} players!",
                        IconName = "party_icon",
                        Timestamp = DateTime.Now
                    });
                }

                if (mainPlayerWon)
                {
                    var secondPlace = sortedPlayers.Count > 1 ? sortedPlayers[1] : null;
                    int margin = secondPlace != null ? (winner.Score - secondPlace.Score) : winner.Score;

                    string winDescription = margin == 1
                        ? "Won multiplayer game by 1 point - what a nail biter!"
                        : $"Won multiplayer game with {winner.Score} points";

                    await _progressService.LogActivityAsync(new ActivityLog
                    {
                        Id = Guid.NewGuid().ToString(),
                        Type = ActivityType.Practice,
                        Description = winDescription,
                        IconName = margin == 1 ? "close_call_icon" : "champion_icon",
                        Timestamp = DateTime.Now
                    });
                }

                await _progressService.UpdateAchievementsAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error logging multiplayer completion: {ex.Message}");
            }
        }

        // NEW: Check for perfect round achievement
        public async void CheckForPerfectRound()
        {
            if (!IsMultiplayer || !Players.All(p => p.HasAnswered && p.GotCurrentAnswerCorrect))
                return;

            await _progressService.LogActivityAsync(new ActivityLog
            {
                Id = Guid.NewGuid().ToString(),
                Type = ActivityType.Practice,
                Description = "All players got the sign correct - team harmony!",
                IconName = "harmony_icon",
                Timestamp = DateTime.Now
            });
        }
        #endregion

        #region Public Methods
        public void LoadNextSign()
        {
            if (!IsGameActive || IsGameOver)
            {
                return;
            }

            Debug.WriteLine($"LoadNextSign started. Available indices: {_availableIndices.Count}");
            IsLoading = true;

            try
            {
                ResetButtonColors();

                if (IsMultiplayer)
                {
                    Debug.WriteLine("=== RESETTING PLAYER STATES FOR NEW SIGN ===");
                    foreach (var player in Players)
                    {
                        player.ResetForNewSign();
                        Debug.WriteLine($"Reset player {player.Name}: HasAnswered = false");
                    }

                    OnPropertyChanged(nameof(Players));
                    OnPropertyChanged(nameof(HasAllPlayersAnswered));
                }

                if (_availableIndices.Count == 0)
                {
                    Debug.WriteLine("No more signs available, ending game");
                    IsGameOver = true;
                    _timer?.Stop();
                    return;
                }

                Random random = new Random();
                int randomIndex = random.Next(_availableIndices.Count);
                int selectedSignIndex = _availableIndices[randomIndex];

                _availableIndices.RemoveAt(randomIndex);
                CurrentSign = _signs[selectedSignIndex];

                if (IsPerformMode)
                {
                    IsSignHidden = true;
                }

                if (IsGuessMode && IsTimerEnabled)
                {
                    StartTimer();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in LoadNextSign: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task LoadVideoForSign(string videoPath)
        {
            if (string.IsNullOrEmpty(videoPath)) return;

            try
            {
                var fullPath = await _videoService.GetVideoPath(videoPath);
                if (!File.Exists(fullPath))
                {
                    Debug.WriteLine("Video file not found!");
                    return;
                }

                var uri = new Uri($"file://{fullPath}");
                var source = MediaSource.FromUri(uri);

                var window = Application.Current?.Windows.FirstOrDefault();
                var gamePage = window?.Page?.Navigation?.NavigationStack.LastOrDefault() as GamePage;
                if (gamePage != null)
                {
                    gamePage.SetVideoSource(source);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading video: {ex.Message}");
            }
        }

        public void Cleanup()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
        }

        public required MediaSource CurrentVideoSource { get; set; }

        public ICommand GoToSettingsCommand => new Command(async () =>
        {
            try
            {
                if (Application.Current?.Windows.FirstOrDefault()?.Page == null)
                {
                    Debug.WriteLine("Navigation service not available");
                    return;
                }

                var themeService = _serviceProvider.GetRequiredService<IThemeService>();
                var settingsPage = new SettingsPage(themeService, _serviceProvider);
                await Shell.Current.Navigation.PushAsync(settingsPage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                var window = Application.Current?.Windows.FirstOrDefault();
                if (window != null)
                {
                    await window.Page.DisplayAlert("Error", "Unable to open settings", "OK");
                }
            }
        });

        public ICommand PlayerCorrectCommand => new Command<Player>(player =>
        {
            if (player == null) return;

            try
            {
                player.RecordAnswer(true);
                FeedbackText = $"{player.Name} got it right! ✓";
                FeedbackBackgroundColor = GetFeedbackColor(true);
                IsFeedbackVisible = true;

                OnPropertyChanged(nameof(Players));
                OnPropertyChanged(nameof(HasAllPlayersAnswered));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in PlayerCorrectCommand: {ex.Message}");
            }
        });

        public ICommand PlayerIncorrectCommand => new Command<Player>(player =>
        {
            if (player == null) return;

            try
            {
                player.RecordAnswer(false);
                FeedbackText = $"{player.Name} got it wrong ✗";
                FeedbackBackgroundColor = GetFeedbackColor(false);
                IsFeedbackVisible = true;

                OnPropertyChanged(nameof(Players));
                OnPropertyChanged(nameof(HasAllPlayersAnswered));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in PlayerIncorrectCommand: {ex.Message}");
            }
        });

        public void ResetGame()
        {
            Debug.WriteLine("ResetGame started");
            IsGameActive = true;
            IsGameOver = false;
            CurrentScore = 0;
            SignsPlayed = 0;
            ResetButtonColors();

            if (IsMultiplayer)
            {
                foreach (var player in Players)
                {
                    player.Score = 0;
                    player.ResetForNewSign();
                }
                OnPropertyChanged(nameof(Players));
            }

            if (_signs == null || !_signs.Any())
            {
                _signs = new SignRepository().GetSigns();
            }

            int questionLimit;
            if (IsGuessMode)
            {
                questionLimit = Preferences.Get(Constants.GUESS_MODE_QUESTIONS_KEY, Constants.DEFAULT_QUESTIONS);
                _availableIndices = Enumerable.Range(0, _signs.Count)
                                          .OrderBy(x => Guid.NewGuid())
                                          .Take(questionLimit)
                                          .ToList();
            }
            else
            {
                questionLimit = GameParameters?.QuestionsCount ?? Constants.DEFAULT_PERFORM_QUESTIONS;
                _availableIndices = Enumerable.Range(0, _signs.Count)
                                          .OrderBy(x => Guid.NewGuid())
                                          .Take(questionLimit)
                                          .ToList();
            }

            Debug.WriteLine($"Available indices count: {_availableIndices.Count}");
            LoadNextSign();
        }

        // UPDATED: EndGame with multiplayer logging
        public void EndGame()
        {
            try
            {
                Debug.WriteLine("EndGame started");
                _timer?.Stop();

                // Log multiplayer-specific activities
                if (IsMultiplayer)
                {
                    LogMultiplayerGameCompletion();
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    IsGameActive = false;
                    IsGameOver = true;

                    if (IsMultiplayer)
                    {
                        var winner = Players.OrderByDescending(p => p.Score).First();
                        GuessResults = $"Winner: {winner.Name}\nScore: {winner.Score} points!";
                    }
                    else
                    {
                        int totalQuestions;
                        if (IsGuessMode)
                        {
                            totalQuestions = Preferences.Get(Constants.GUESS_MODE_QUESTIONS_KEY, Constants.DEFAULT_QUESTIONS);
                        }
                        else
                        {
                            totalQuestions = GameParameters?.QuestionsCount ?? Constants.DEFAULT_PERFORM_QUESTIONS;
                        }

                        GuessResults = $"Final Score: {CurrentScore}/{totalQuestions}";
                    }

                    OnPropertyChanged(nameof(IsGameOver));
                    OnPropertyChanged(nameof(GuessResults));
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error ending game: {ex.Message}");
            }
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Multiplayer Support
        private GameParameters _gameParameters = new()
        {
            IsMultiplayer = false,
            Players = new List<Player> { new Player { Name = "You", IsMainPlayer = true } }
        };

        public GameParameters GameParameters
        {
            get => _gameParameters;
            set
            {
                if (_gameParameters != value)
                {
                    _gameParameters = value;
                    InitializePlayers();
                    OnPropertyChanged(nameof(GameParameters));
                    OnPropertyChanged(nameof(IsMultiplayer));
                }
            }
        }

        private void InitializePlayers()
        {
            // Unsubscribe from previous players to avoid duplicates/leaks
            foreach (var existing in Players.ToList())
            {
                UnsubscribeFromPlayer(existing);
            }

            Players.Clear();

            if (GameParameters?.Players != null)
            {
                foreach (var player in GameParameters.Players)
                {
                    Players.Add(player);
                    SubscribeToPlayer(player);
                }
            }
            else
            {
                var you = new Player { Name = "You", IsMainPlayer = true };
                Players.Add(you);
                SubscribeToPlayer(you);
            }

            if (IsMultiplayer)
            {
                var mainPlayer = Players.FirstOrDefault(p => p.IsMainPlayer);
                CurrentPlayerTurnText = mainPlayer != null ? $"{mainPlayer.Name}'s Turn" : "Player 1's Turn";
            }

            // Ensure UI knows to update
            OnPropertyChanged(nameof(Players));
            OnPropertyChanged(nameof(IsMultiplayer));
            OnPropertyChanged(nameof(CurrentPlayerTurnText));
            OnPropertyChanged(nameof(ModeTitle));
            OnPropertyChanged(nameof(HasAllPlayersAnswered));
        }

        private async Task ShowResultsConfirmation()
        {
            var correctPlayers = Players.Where(p => p.HasAnswered && p.GotCurrentAnswerCorrect).ToList();
            var incorrectPlayers = Players.Where(p => p.HasAnswered && !p.GotCurrentAnswerCorrect).ToList();
            var allCorrect = Players.All(p => p.GotCurrentAnswerCorrect);

            bool isSoberMode = Preferences.Get(Constants.SOBER_MODE_KEY, false);

            string title;
            string message;

            if (allCorrect)
            {
                title = "Perfect Round!";
                message = "Everyone signed correctly! Keep up the great work!";
            }
            else
            {
                title = "Round Complete";

                if (correctPlayers.Any())
                {
                    var correctNames = string.Join(", ", correctPlayers.Select(p => p.Name));
                    message = $"Great job: {correctNames}!\n\n";
                }
                else
                {
                    message = "";
                }

                if (incorrectPlayers.Any())
                {
                    var incorrectNames = string.Join(", ", incorrectPlayers.Select(p => p.Name));
                    message += isSoberMode
                        ? $"Keep practicing: {incorrectNames}"
                        : $"Time for a sip: {incorrectNames}";
                }
            }

            await Application.Current.MainPage.DisplayAlert(title, message.Trim(), "Next Sign");
        }

        private async Task ShowGuessResults()
        {
            var correctPlayers = Players.Where(p => p.HasAnswered && p.GotCurrentAnswerCorrect).ToList();
            var incorrectPlayers = Players.Where(p => p.HasAnswered && !p.GotCurrentAnswerCorrect).ToList();
            var allCorrect = Players.All(p => p.GotCurrentAnswerCorrect);

            bool isSoberMode = Preferences.Get(Constants.SOBER_MODE_KEY, false);

            string title;
            string message;

            if (allCorrect)
            {
                title = "Perfect Round!";
                message = $"Everyone got it right!\n\nThe answer was: {CurrentSign?.CorrectAnswer}";
            }
            else
            {
                title = $"Correct Answer: {CurrentSign?.CorrectAnswer}";
                message = "";

                if (correctPlayers.Any())
                {
                    // Show only player names for correct players (remove "(selected ...)")
                    var correctNames = string.Join(", ", correctPlayers.Select(p => p.Name));
                    message = $"✓ Correct: {correctNames}\n\n";
                }

                if (incorrectPlayers.Any())
                {
                    var incorrectNames = string.Join(", ", incorrectPlayers.Select(p => p.Name));

                    message += isSoberMode
                            ? $"✗ Incorrect: {incorrectNames}"
                            : $"✗ Take a sip: {incorrectNames}";
                }
            }

            // Show color-coded feedback overlay first
            FeedbackText = title + "\n\n" + CurrentSign?.CorrectAnswer;
            FeedbackBackgroundColor = allCorrect ?
                Color.FromArgb("#28a745") :
                Color.FromArgb("#007BFF");
            IsFeedbackVisible = true;

            // Then show detailed alert
            await Application.Current.MainPage.DisplayAlert(title, message.Trim(), "Next Sign");

            IsFeedbackVisible = false;
        }

        private void SubscribeToPlayer(Player player)
        {
            if (player == null) return;
            player.PropertyChanged += OnPlayerPropertyChanged;
        }

        private void UnsubscribeFromPlayer(Player player)
        {
            if (player == null) return;
            player.PropertyChanged -= OnPlayerPropertyChanged;
        }

        private void OnPlayerPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // When a player's selection/answer state changes, notify that HasAllPlayersAnswered (and Players) changed
            if (e.PropertyName == nameof(Player.SelectedAnswer) ||
                e.PropertyName == nameof(Player.HasAnswered) ||
                e.PropertyName == nameof(Player.SelectedAnswerNumber))
            {
                OnPropertyChanged(nameof(HasAllPlayersAnswered));
                OnPropertyChanged(nameof(Players));
            }
        }

        public ICommand ConfirmResultsCommand
        {
            get
            {
                return _confirmResultsCommand ??= new Command(async () =>
                {
                    if (IsProcessingAnswer) return;

                    if (IsMultiplayer && !HasAllPlayersAnswered)
                    {
                        var unansweredPlayers = Players.Where(p => !p.HasAnswered).ToList();
                        var playerNames = string.Join(", ", unansweredPlayers.Select(p => p.Name));

                        await Application.Current.MainPage.DisplayAlert(
                            "Waiting for Players",
                            $"Still waiting for: {playerNames}\n\nMake sure all players have recorded their answers.",
                            "OK");
                        return;
                    }

                    try
                    {
                        IsProcessingAnswer = true;

                        var mainPlayer = Players.FirstOrDefault(p => p.IsMainPlayer);
                        if (mainPlayer != null && mainPlayer.HasAnswered)
                        {
                            bool isCorrect = mainPlayer.GotCurrentAnswerCorrect;
                            await LogGameActivity(isCorrect);

                            if (isCorrect)
                            {
                                CurrentScore++;
                            }
                        }

                        // Check for perfect round achievement
                        CheckForPerfectRound();

                        await ShowResultsConfirmation();

                        SignsPlayed++;

                        if (IsPerformMode)
                        {
                            IsSignHidden = true;
                        }

                        if (_availableIndices.Count > 0)
                        {
                            LoadNextSign();
                        }
                        else
                        {
                            EndGame();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error in ConfirmResultsCommand: {ex.Message}");
                        await Application.Current.MainPage.DisplayAlert("Error",
                            "There was an error loading the next sign. Please try again.", "OK");
                    }
                    finally
                    {
                        IsProcessingAnswer = false;
                    }
                });
            }
        }

        public void RecordPlayerAnswer(PlayerAnswerParameter param)
        {
            if (param?.Player == null || IsProcessingAnswer) return;

            try
            {
                IsProcessingAnswer = true;
                param.Player.GotCurrentAnswerCorrect = param.IsCorrect;

                if (param.IsCorrect)
                {
                    param.Player.Score++;

                    if (param.Player.IsMainPlayer)
                    {
                        CurrentScore++;
                        _ = LogGameActivity(true);
                    }
                }
                else if (param.Player.IsMainPlayer)
                {
                    _ = LogGameActivity(false);
                }

                OnPropertyChanged(nameof(Players));
                OnPropertyChanged(nameof(HasAllPlayersAnswered));

                FeedbackText = $"{param.Player.Name} {(param.IsCorrect ? "got it right! ✓" : "got it wrong ✗")}";
                FeedbackBackgroundColor = GetFeedbackColor(param.IsCorrect);
                IsFeedbackVisible = true;

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Task.Delay(1500);
                    IsFeedbackVisible = false;
                });
            }
            finally
            {
                IsProcessingAnswer = false;
            }
        }

        public ICommand ShowScoreboardCommand
        {
            get
            {
                return _showScoreboardCommand ??= new Command(() =>
                {
                    var scoreboard = $"Signs Played: {SignsPlayed}\n\nCurrent Scores:\n\n";
                    foreach (var player in Players.OrderByDescending(p => p.Score))
                    {
                        scoreboard += $"{player.Name}: {player.Score} points\n";
                    }

                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert("Scoreboard", scoreboard, "Close");
                    });
                });
            }
        }
        #endregion
    }
}