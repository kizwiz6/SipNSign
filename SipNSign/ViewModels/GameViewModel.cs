using com.kizwiz.sipnsign.Enums;
using com.kizwiz.sipnsign.Models;
using com.kizwiz.sipnsign.Pages;
using com.kizwiz.sipnsign.Services;
using CommunityToolkit.Maui.Views;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace com.kizwiz.sipnsign.ViewModels
{
    /// <summary>
    /// Manages the game state and logic for both Guess and Perform modes
    /// </summary>
    public class GameViewModel : INotifyPropertyChanged
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
        private string _feedbackBackgroundColor;
        private string _guessResults;
        private bool _isFeedbackVisible;
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
        /// <summary>
        /// Event triggered when a sign reveal is requested.
        /// </summary>
        public event EventHandler? SignRevealRequested;
        public Color FeedbackSuccessColor => _successColor.WithAlpha(0.9f);
        public Color FeedbackErrorColor => _errorColor.WithAlpha(0.9f);
        public string ModeTitle => _currentMode == GameMode.Guess ? "Guess Mode" : "Perform Mode";
        public bool IsTimerEnabled => Preferences.Get(Constants.TIMER_DURATION_KEY, Constants.DEFAULT_TIMER_DURATION) > 0;

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

        public string FeedbackBackgroundColor
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
            _feedbackBackgroundColor = string.Empty;
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
            FeedbackBackgroundColor = Colors.Transparent.ToArgbHex();
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
                UpdateButtonColor(answer, isCorrect);

                // Only show feedback if enabled
                if (Preferences.Get(Constants.SHOW_FEEDBACK_KEY, true))
                {
                    FeedbackText = GetFeedbackText(isCorrect);
                    string color = GetFeedbackColor(isCorrect);
                    Debug.WriteLine($"Setting feedback color to: {color}");
                    FeedbackBackgroundColor = color;
                    IsFeedbackVisible = true;
                }

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
                Debug.WriteLine($"Is last question: {isLastQuestion}");

                await ShowFeedbackAndContinue(isCorrect);
                Debug.WriteLine("After ShowFeedbackAndContinue");

                if (isLastQuestion)
                {
                    Debug.WriteLine("Ending game");
                    EndGame();
                    Debug.WriteLine("Game ended");
                }
            }
            finally
            {
                IsProcessingAnswer = false;
            }
        }

        public string GetFeedbackColor(bool isCorrect)
        {
            bool useTransparent = Preferences.Get(Constants.TRANSPARENT_FEEDBACK_KEY, false);
            string color = useTransparent ? "#80000000" : (isCorrect ? "#28a745" : "#dc3545");
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
            FeedbackText = GetFeedbackText(isCorrect);
            IsFeedbackVisible = true;
            Debug.WriteLine($"Showing feedback - Mode: {CurrentMode}, Color: {FeedbackBackgroundColor}");
        }

        private void UpdateAnswerTime(double time)
        {
            _answerTimes.Add(time);
            _averageAnswerTime = _answerTimes.Average();
        }

        public void RevealSign()
        {
            _logger.Debug($"RevealSign called. CurrentSign is: {CurrentSign?.CorrectAnswer ?? "null"}");
            IsSignHidden = false;
            SignRevealRequested?.Invoke(this, EventArgs.Empty);
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
            if (IsProcessingAnswer) return;  // Prevent multiple clicks

            try
            {
                IsProcessingAnswer = true;

                // Log transparency setting for debugging
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
            if (IsProcessingAnswer) return;  // Prevent multiple clicks

            try
            {
                IsProcessingAnswer = true;

                // Log transparency setting for debugging
                Debug.WriteLine($"HandleCorrectPerform - Current transparency setting: {Preferences.Get(Constants.TRANSPARENT_FEEDBACK_KEY, false)}");

                IsSignHidden = true;
                _timer.Stop();
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
        /// <param name="isCorrect">Whether the previous answer was correct</param>
        private async Task ShowFeedbackAndContinue(bool isCorrect)
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

                // Only load next sign if we have more questions
                if (_availableIndices.Count > 0)
                {
                    Debug.WriteLine("Loading next sign");
                    LoadNextSign();
                }
                else
                {
                    Debug.WriteLine("No more questions available");
                }
            }
            Debug.WriteLine("ShowFeedbackAndContinue completed");
        }

        private void UpdateButtonColor(string answer, bool isCorrect)
        {
            // First, find the correct answer button
            int correctAnswerIndex = -1;
            for (int i = 0; i < 4; i++)
            {
                if (CurrentSign?.Choices[i] == CurrentSign?.CorrectAnswer)
                {
                    correctAnswerIndex = i;
                    break;
                }
            }

            // Find which button was clicked
            int selectedAnswerIndex = -1;
            for (int i = 0; i < 4; i++)
            {
                if (CurrentSign?.Choices[i] == answer)
                {
                    selectedAnswerIndex = i;
                    break;
                }
            }

            // Set colors for all buttons
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
                Debug.WriteLine($"ButtonBaseColor value: {ButtonBaseColor}");
                Button1Color = ButtonBaseColor;
                Button2Color = ButtonBaseColor;
                Button3Color = ButtonBaseColor;
                Button4Color = ButtonBaseColor;
            }

            Debug.WriteLine($"Final Button1Color: {Button1Color}");
        }

        /// <summary>
        /// Initializes and starts a timer with a specific duration.
        /// The timer ticks every second, and the remaining time is updated accordingly.
        /// If the timer duration is set to 0 (disabled), the timer will not start.
        /// </summary>
        private void StartTimer()
        {
            try
            {
                // Check if _timer is null and needs to be created
                if (_timer == null)
                {
                    Debug.WriteLine("Creating new timer");

                    // Check if Application.Current and Dispatcher are non-null
                    var dispatcher = Application.Current?.Dispatcher;
                    if (dispatcher == null)
                    {
                        Debug.WriteLine("Dispatcher is null. Cannot create timer.");
                        return; // If Dispatcher is null, don't try to create the timer
                    }

                    _timer = dispatcher.CreateTimer();
                    if (_timer == null)
                    {
                        Debug.WriteLine("Failed to create timer.");
                        return; // Exit if timer creation fails
                    }

                    _timer.Interval = TimeSpan.FromSeconds(1);
                    _timer.Tick += Timer_Tick;
                }

                // Get the timer duration from preferences
                int duration = Preferences.Get(Constants.TIMER_DURATION_KEY, Constants.DEFAULT_TIMER_DURATION);
                if (duration > 0)  // Start the timer if duration is not zero (disabled)
                {
                    RemainingTime = duration;
                    _timer?.Start();
                    Debug.WriteLine($"Timer started with duration: {duration}");
                }
                else
                {
                    RemainingTime = 0; // Hide the timer if duration is 0
                    Debug.WriteLine("Timer disabled.");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions gracefully and log them
                Debug.WriteLine($"Error in StartTimer: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Updates the practice time for tracking achievements
        /// </summary>
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

        /// <summary>
        /// Logs game activity and updates progress when signs are completed
        /// </summary>
        /// <param name="isCorrect">Whether the sign was correctly identified/performed</param>
        private async Task LogGameActivity(bool isCorrect, double? answerTime = null)
        {
            if (_userProgress == null)
            {
                _userProgress = await _progressService.GetUserProgressAsync();
            }

            // Update total attempts and correct attempts
            _userProgress.TotalAttempts++;

            if (isCorrect)
            {
                _userProgress.SignsLearned++;
                _userProgress.CorrectAttempts++;
                _userProgress.CorrectInARow++;

                // Update mode-specific counters
                if (CurrentMode == GameMode.Guess)
                {
                    _userProgress.GuessModeSigns++;
                }
                else
                {
                    _userProgress.PerformModeSigns++;
                }
            }
            else
            {
                _userProgress.CorrectInARow = 0;  // Reset streak on wrong answer
            }

            // Calculate and update accuracy
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

            // Check for perfect session
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

            // For Rapid Fire achievement
            if (isCorrect && answerTime.HasValue && answerTime.Value < 5.0)
            {
                await _progressService.LogActivityAsync(new ActivityLog
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = ActivityType.Practice,
                    Description = $"Answered '{CurrentSign?.CorrectAnswer}' correctly under 5 seconds",
                    IconName = "speed_icon",
                    Timestamp = DateTime.Now,
                    Score = "+1"
                });
            }

            // At the end of a session (when all questions are answered)
            if (_availableIndices.Count == 0)
            {
                // For Party Starter achievement (Perform Mode)
                if (CurrentMode == GameMode.Perform)
                {
                    await _progressService.LogActivityAsync(new ActivityLog
                    {
                        Id = Guid.NewGuid().ToString(),
                        Type = ActivityType.Practice,
                        Description = "Perform Mode session completed",
                        IconName = "party_icon",
                        Timestamp = DateTime.Now
                    });
                }

                // For Speed Master achievement (Guess Mode)
                if (CurrentMode == GameMode.Guess && _averageAnswerTime < 3.0)
                {
                    await _progressService.LogActivityAsync(new ActivityLog
                    {
                        Id = Guid.NewGuid().ToString(),
                        Type = ActivityType.Practice,
                        Description = "Guess Mode completed with average time under 3 seconds",
                        IconName = "speed_master_icon",
                        Timestamp = DateTime.Now
                    });
                }
            }
        }

        private async Task LogAchievementActivity(string achievementTitle)
        {
            await _progressService.LogActivityAsync(new ActivityLog
            {
                Id = Guid.NewGuid().ToString(),
                Type = ActivityType.Achievement,
                Description = $"Unlocked: {achievementTitle}",
                IconName = "achievement_icon.svg",
                Timestamp = DateTime.Now
            });
        }

        #endregion

        #region Public Methods
        public void LoadNextSign()
        {
            // Don't load next sign if game is over
            if (!IsGameActive || IsGameOver)
            {
                return;
            }

            Debug.WriteLine($"LoadNextSign started. Available indices: {_availableIndices.Count}");
            IsLoading = true;

            try
            {
                // Reset button colors before loading new sign
                ResetButtonColors();

                // Check if we've reached the question limit
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

                _logger.Debug($"LoadNextSign: Loading sign at index {selectedSignIndex}");
                _logger.Debug($"LoadNextSign: Sign word is: {_signs[selectedSignIndex].CorrectAnswer}");

                // Remove the index before setting CurrentSign to prevent reuse
                _availableIndices.RemoveAt(randomIndex);

                // Add this line to debug Perform Mode state
                if (IsPerformMode)
                {
                    Debug.WriteLine($"LoadNextSign: In Perform Mode. IsSignHidden: {IsSignHidden}, Word to show: {CurrentSign?.CorrectAnswer}");
                }

                CurrentSign = _signs[selectedSignIndex];

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

                Debug.WriteLine("Creating settings page");
                var themeService = _serviceProvider.GetRequiredService<IThemeService>();
                var settingsPage = new SettingsPage(themeService, _serviceProvider);
                Debug.WriteLine("Pushing settings page");
                await Shell.Current.Navigation.PushAsync(settingsPage);
                Debug.WriteLine("Navigation completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                var window = Application.Current?.Windows.FirstOrDefault();
                if (window != null)
                {
                    await window.Page.DisplayAlert("Error", "Unable to open settings", "OK");
                }
            }
        });

        public void ResetGame()
        {
            Debug.WriteLine("ResetGame started");
            IsGameActive = true;
            IsGameOver = false;
            CurrentScore = 0;
            ResetButtonColors();

            if (_signs == null || !_signs.Any())
            {
                Debug.WriteLine("Warning: Signs list is empty or null");
                _signs = new SignRepository().GetSigns();
            }

            int questionLimit = Preferences.Get(Constants.GUESS_MODE_QUESTIONS_KEY, Constants.DEFAULT_QUESTIONS);
            Debug.WriteLine($"Setting up game with {questionLimit} questions");

            if (IsGuessMode)
            {
                _availableIndices = Enumerable.Range(0, _signs.Count)
                                            .OrderBy(x => Guid.NewGuid())
                                            .Take(questionLimit)
                                            .ToList();
            }
            else
            {
                _availableIndices = Enumerable.Range(0, _signs.Count).ToList();
            }

            LoadNextSign();
        }

        /// <summary>
        /// Ends the current game, stops the timer, and displays the user's results.
        /// </summary>
        /// <remarks>
        /// This method stops the timer, marks the game as over, and calculates the number of correct guesses
        /// based on the total number of questions. The results are displayed in a user-friendly format.
        /// </remarks>
        public void EndGame()
        {
            Debug.WriteLine("EndGame started");
            _timer?.Stop();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsGameActive = false;
                IsGameOver = true;

                // Set the results text
                int totalQuestions = Preferences.Get(Constants.GUESS_MODE_QUESTIONS_KEY, Constants.DEFAULT_QUESTIONS);
                GuessResults = $"Final Score: {CurrentScore}/{totalQuestions}";
                Debug.WriteLine("EndGame completed");

                // Force UI update
                OnPropertyChanged(nameof(IsGameOver));
                OnPropertyChanged(nameof(GuessResults));
            });
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}