using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.ComponentModel;
using System.Windows.Input;
using com.kizwiz.sipnsign.Models;
using com.kizwiz.sipnsign.Enums;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using com.kizwiz.sipnsign.Services;

namespace com.kizwiz.sipnsign.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        #region Color Constants
        private readonly Color _guessPrimaryColor = Color.FromArgb("#007BFF");
        private readonly Color _performPrimaryColor = Color.FromArgb("#28a745");
        private readonly Color _successColor = Color.FromArgb("#28a745");
        private readonly Color _errorColor = Color.FromArgb("#dc3545");
        #endregion

        #region Private Fields
        private readonly IDispatcherTimer _timer;
        private readonly ILoggingService _logger;
        private readonly IProgressService _progressService;
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
        private bool _isFeedbackVisible;
        private int _finalScore;
        private Color _button1Color;
        private Color _button2Color;
        private Color _button3Color;
        private Color _button4Color;
        private GameMode _currentMode = GameMode.Guess;
        private ICommand _playAgainCommand;
        private bool _isSignHidden = true;
        private string _guessResults;
        private UserProgress _userProgress;
        #endregion

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
                if (_isGameActive != value)
                {
                    _isGameActive = value;
                    OnPropertyChanged(nameof(IsGameActive));
                }
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
        public Color ButtonBaseColor => PrimaryColor;
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
                    _logger.Debug($"Mode changing from {_currentMode} to {value}");
                    _currentMode = value;
                    _timer.Stop();
                    RemainingTime = 0;

                    OnPropertyChanged(nameof(CurrentMode));
                    OnPropertyChanged(nameof(PrimaryColor));
                    OnPropertyChanged(nameof(ProgressBarColor));
                    OnPropertyChanged(nameof(ButtonBaseColor));
                    OnPropertyChanged(nameof(ModeTitle));
                    OnPropertyChanged(nameof(IsGuessMode));
                    OnPropertyChanged(nameof(IsPerformMode));
                    ResetButtonColors();
                    ResetGame();
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
                    Debug.WriteLine($"CurrentSign changed to: {value?.CorrectAnswer ?? "null"}");
                    OnPropertyChanged(nameof(CurrentSign));
                }
            }
        }

        public bool IsGameOver
        {
            get => _isGameOver;
            set
            {
                if (_isGameOver != value)
                {
                    _isGameOver = value;
                    if (value)
                    {
                        FinalScore = CurrentScore;
                    }
                    OnPropertyChanged(nameof(IsGameOver));
                }
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
            get => _isFeedbackVisible;
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

        private Color GetButtonColor(int buttonIndex, int selectedIndex, int correctIndex, bool isCorrect)
        {
            if (IsProcessingAnswer)
            {
                if (buttonIndex == selectedIndex)
                {
                    // Selected button
                    return isCorrect ? FeedbackSuccessColor : FeedbackErrorColor;
                }
                else if (buttonIndex == correctIndex && !isCorrect)
                {
                    // Show correct answer when wrong
                    return FeedbackSuccessColor;
                }
            }

            // Keep original color for unselected buttons or when not processing
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
        public ICommand AnswerCommand { get; private set; }
        public ICommand PlayAgainCommand
        {
            get
            {
                return _playAgainCommand ??= new Command(() =>
                {
                    IsFeedbackVisible = false;
                    IsGameOver = false;
                    IsGameActive = true;
                    ResetGame();
                });
            }
        }
        public ICommand VideoLoadedCommand { get; private set; }
        public ICommand RevealSignCommand { get; private set; }
        public ICommand CorrectPerformCommand { get; private set; }
        public ICommand IncorrectPerformCommand { get; private set; }
        public ICommand SwitchModeCommand { get; private set; }
        #endregion

        #region Events
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion

        #region Constructor
        public GameViewModel(ILoggingService logger, IProgressService progressService)
        {
            Debug.WriteLine("GameViewModel constructor started");
            try
            {
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
                _progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
                Debug.WriteLine("Services initialised");
                _correctInARow = 0;

                // Initialize user progress first
                _userProgress = Task.Run(async () => await _progressService.GetUserProgressAsync()).Result;
                if (_userProgress == null)
                {
                    throw new InvalidOperationException("Failed to initialize user progress");
                }

                // Load sign data
                SignRepository signRepository = new SignRepository();
                _signs = signRepository.GetSigns();
                Debug.WriteLine($"Loaded {_signs.Count} signs");

                if (!_signs.Any())
                {
                    Debug.WriteLine("No signs loaded");
                    throw new InvalidOperationException("No signs were loaded");
                }

                foreach (var sign in _signs)
                {
                    Debug.WriteLine($"Video path: {sign.VideoPath}");
                }

                // Initialize game state
                _availableIndices = new List<int>();
                _feedbackBackgroundColor = Colors.Transparent.ToArgbHex();

                // Setup timer
                _timer = Application.Current.Dispatcher.CreateTimer();
                _timer.Interval = TimeSpan.FromSeconds(1);
                _timer.Tick += Timer_Tick;

                // Initialize UI and game
                InitializeCommands();
                ResetButtonColors();
                InitializeGame();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GameViewModel constructor: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Let the UI handle this
            }
        }
        #endregion

        #region Private Methods
        private void InitializeCommands()
        {
            AnswerCommand = new Command<string>(HandleAnswer);
            _playAgainCommand = new Command(ResetGame);
            VideoLoadedCommand = new Command(() => IsLoading = false);
            RevealSignCommand = new Command(RevealSign);
            CorrectPerformCommand = new Command(HandleCorrectPerform);
            IncorrectPerformCommand = new Command(HandleIncorrectPerform);
            SwitchModeCommand = new Command<GameMode>(SwitchMode);
        }

        private void InitializeGame()
        {
            ProgressPercentage = 0;
            IsFeedbackVisible = false;
            FeedbackBackgroundColor = Colors.Transparent.ToArgbHex();
            ResetGame();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (RemainingTime > 0)
            {
                RemainingTime--;
            }
            else
            {
                _timer.Stop();
                HandleTimeOut();
            }
        }

        /// <summary>
        /// Handles when the timer runs out in Guess Mode
        /// </summary>
        private void HandleTimeOut()
        {
            IsProcessingAnswer = true;
            FeedbackText = $"Time's up!\n\nThe sign means '{CurrentSign?.CorrectAnswer}'.\n\nTake a sip!";
            FeedbackBackgroundColor = FeedbackErrorColor.ToArgbHex();
            IsFeedbackVisible = true;

            Task.Delay(3000).ContinueWith(_ =>
            {
                IsFeedbackVisible = false;
                IsProcessingAnswer = false;
                LoadNextSign();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private bool CheckAnswer(string answer)
        {
            return answer == CurrentSign?.CorrectAnswer;
        }

        private async void HandleAnswer(string answer)
        {
            if (!IsGameActive || IsGameOver || IsProcessingAnswer) return;  // Check if game is still active
            if (IsProcessingAnswer) return; // prevent multiple clicks

            try
            {
                IsProcessingAnswer = true;
                _timer.Stop();
                bool isCorrect = CheckAnswer(answer);
                UpdateButtonColor(answer, isCorrect);

                if (isCorrect)
                {
                    FeedbackText = $"Correct!\n\nThe sign means '{CurrentSign?.CorrectAnswer}'.";
                    FeedbackBackgroundColor = FeedbackSuccessColor.ToArgbHex();
                    CurrentScore++;
                    await LogGameActivity(true);
                }
                else
                {
                    FeedbackText = $"Incorrect.\n\nThe sign means '{CurrentSign?.CorrectAnswer}'.\n\nTake a sip!";
                    FeedbackBackgroundColor = FeedbackErrorColor.ToArgbHex();
                    await LogGameActivity(false);
                }

                await ShowFeedbackAndContinue(isCorrect);
            }
            finally
            {
                IsProcessingAnswer = false;
            }
        }

        private void RevealSign()
        {
            _logger.Debug($"RevealSign called. CurrentSign is: {CurrentSign?.CorrectAnswer ?? "null"}");
            IsSignHidden = false;
        }

        private async Task HandleCorrectAnswer()
        {
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
                _timer.Stop();
                CurrentScore++;
                FeedbackText = "Nice work!\n\nPrepare for your next sign!";
                FeedbackBackgroundColor = FeedbackSuccessColor.ToArgbHex();
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
                _timer.Stop();
                FeedbackText = $"Remember to practice '{CurrentSign?.CorrectAnswer}'!\n\nTake a sip!";
                FeedbackBackgroundColor = FeedbackErrorColor.ToArgbHex();
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
            if (IsGameOver) return;  // Don't show feedback if game is over

            IsFeedbackVisible = true;
            FeedbackBackgroundColor = isCorrect ? FeedbackSuccessColor.ToArgbHex() : FeedbackErrorColor.ToArgbHex();

            // Use shorter delay for correct answers, configurable delay for incorrect
            int delay = isCorrect ? 2000 : Preferences.Get(Constants.INCORRECT_DELAY_KEY, Constants.DEFAULT_DELAY);
            await Task.Delay(delay);

            if (!IsGameOver)  // Check again in case game ended during delay
            {
                IsFeedbackVisible = false;
                if (IsPerformMode) IsSignHidden = true;
                LoadNextSign();
            }
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
            Button1Color = ButtonBaseColor;
            Button2Color = ButtonBaseColor;
            Button3Color = ButtonBaseColor;
            Button4Color = ButtonBaseColor;
        }

        private void StartTimer()
        {
            int duration = Preferences.Get(Constants.TIMER_DURATION_KEY, Constants.DEFAULT_TIMER_DURATION);
            if (duration > 0)  // Only start timer if duration is not 0 (disabled)
            {
                RemainingTime = duration;
                _timer.Start();
            }
            else
            {
                RemainingTime = 0;  // Hide the timer display
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
        private async Task LogGameActivity(bool isCorrect)
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
                    $"Practiced '{CurrentSign?.CorrectAnswer}'",
                IconName = isCorrect ? "quiz_correct_icon" : "quiz_incorrect_icon",
                Timestamp = DateTime.Now,
                Score = isCorrect ? "+1" : "Try Again"
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
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Game Over",
                            $"Your final score is {CurrentScore}!",
                            "OK"
                        );
                    });
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

        public void ResetGame()
        {
            Debug.WriteLine("ResetGame started");
            IsGameActive = true;  // Re-enable interactions
            CurrentScore = 0;
            _correctInARow = 0;

            int questionLimit = Preferences.Get(Constants.GUESS_MODE_QUESTIONS_KEY, Constants.DEFAULT_QUESTIONS);
            Debug.WriteLine($"Setting up game with {questionLimit} questions");

            if (IsGuessMode)
            {
                var shuffledIndices = Enumerable.Range(0, _signs.Count)
                                              .OrderBy(x => Guid.NewGuid())
                                              .Take(questionLimit)
                                              .ToList();
                _availableIndices = shuffledIndices;
            }
            else
            {
                _availableIndices = Enumerable.Range(0, _signs.Count).ToList();
            }

            Debug.WriteLine($"Available indices count: {_availableIndices.Count}");

            IsGameOver = false;
            FeedbackText = string.Empty;
            IsFeedbackVisible = false;
            ProgressPercentage = 0;
            FeedbackBackgroundColor = Colors.Transparent.ToArgbHex();
            ResetButtonColors();

            // Clear the video source when resetting
           // VideoSource = null;
            IsLoading = true;  // Ensure loading indicator shows

            if (IsPerformMode)
            {
                IsSignHidden = true;
            }

            LoadNextSign();
        }

        private async void EndGame()
        {
            _timer?.Stop();
            IsGameActive = false;
            IsGameOver = true;

            // Set the results text
            int totalQuestions = Preferences.Get(Constants.GUESS_MODE_QUESTIONS_KEY, Constants.DEFAULT_QUESTIONS);
            GuessResults = $"You guessed {CurrentScore}/{totalQuestions} correctly!";

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Game Over",
                    $"Your final score is {CurrentScore}!",
                    "OK"
                );
            });
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}