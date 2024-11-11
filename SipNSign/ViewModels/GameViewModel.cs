using com.kizwiz.sipnsign.Models;
using com.kizwiz.sipnsign.Enums;
using System.ComponentModel;
using System.Windows.Input;

namespace com.kizwiz.sipnsign.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        // Color constants
        private readonly Color _guessPrimaryColor = Color.FromArgb("#007BFF");
        private readonly Color _performPrimaryColor = Color.FromArgb("#28a745");
        private readonly Color _successColor = Color.FromArgb("#28a745");
        private readonly Color _errorColor = Color.FromArgb("#dc3545");

        // Existing fields
        private readonly IDispatcherTimer _timer;
        private const int QuestionTimeLimit = 10;
        private int _remainingTime;
        private bool _isLoading;
        private int _currentScore;
        private SignModel? _currentSign;
        private List<SignModel> _signs;
        private List<int> _availableIndices;
        private string _feedbackText = string.Empty;
        private Color _feedbackColor;
        private bool _isGameOver;
        private double _progressPercentage;
        private string _feedbackBackgroundColor;
        private bool _isFeedbackVisible;
        private int _finalScore;
        private Color _button1Color;
        private Color _button2Color;
        private Color _button3Color;
        private GameMode _currentMode = GameMode.Guess;
        private bool _isSignHidden = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        // Color-related properties
        public Color PrimaryColor => _currentMode == GameMode.Guess ? _guessPrimaryColor : _performPrimaryColor;

        public Color ProgressBarColor => PrimaryColor;

        public Color ButtonBaseColor => PrimaryColor;

        public Color FeedbackSuccessColor => _successColor.WithAlpha(0.9f);

        public Color FeedbackErrorColor => _errorColor.WithAlpha(0.9f);

        public string ModeTitle => _currentMode == GameMode.Guess ? "Guess & Gulp" : "Sign & Sip";

        public GameMode CurrentMode
        {
            get => _currentMode;
            set
            {
                if (_currentMode != value)
                {
                    _currentMode = value;
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

        // Existing properties remain the same but use new color properties
        public Color Button1Color
        {
            get => _button1Color;
            set
            {
                if (_button1Color != value)
                {
                    _button1Color = value;
                    OnPropertyChanged(nameof(Button1Color));
                }
            }
        }

        public Color Button2Color
        {
            get => _button2Color;
            set
            {
                if (_button2Color != value)
                {
                    _button2Color = value;
                    OnPropertyChanged(nameof(Button2Color));
                }
            }
        }

        public Color Button3Color
        {
            get => _button3Color;
            set
            {
                if (_button3Color != value)
                {
                    _button3Color = value;
                    OnPropertyChanged(nameof(Button3Color));
                }
            }
        }

        public GameViewModel()
        {
            SignRepository signRepository = new SignRepository();
            _signs = signRepository.GetSigns() ?? new List<SignModel>();
            _availableIndices = new List<int>();
            _feedbackColor = Colors.Transparent;

            // Initialize timer
            _timer = Application.Current.Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;

            // Initialize commands
            AnswerCommand = new Command<string>(HandleAnswer);
            PlayAgainCommand = new Command(ResetGame);
            VideoLoadedCommand = new Command(() => IsLoading = false);
            RevealSignCommand = new Command(RevealSign);
            CorrectPerformCommand = new Command(HandleCorrectPerform);
            IncorrectPerformCommand = new Command(HandleIncorrectPerform);
            SwitchModeCommand = new Command<GameMode>(SwitchMode);

            // Initialise colors
            ResetButtonColors();

            // Initialize properties
            ProgressPercentage = 0;
            IsFeedbackVisible = false;
            FeedbackBackgroundColor = Colors.Transparent;

            // Start the game
            ResetGame();
        }

        private void ResetButtonColors()
        {
            Button1Color = ButtonBaseColor;
            Button2Color = ButtonBaseColor;
            Button3Color = ButtonBaseColor;
        }

        private void UpdateButtonColor(string answer, bool isCorrect)
        {
            Color newColor = isCorrect ? FeedbackSuccessColor : FeedbackErrorColor;

            if (CurrentSign?.Choices[0] == answer) Button1Color = newColor;
            if (CurrentSign?.Choices[1] == answer) Button2Color = newColor;
            if (CurrentSign?.Choices[2] == answer) Button3Color = newColor;
        }

        private void HandleAnswer(string answer)
        {
            _timer.Stop();

            bool isCorrect = CheckAnswer(answer);
            UpdateButtonColor(answer, isCorrect);

            if (isCorrect)
            {
                FeedbackText = "Correct!";
                FeedbackBackgroundColor = FeedbackSuccessColor.ToArgbHex();
                CurrentScore++;
            }
            else
            {
                FeedbackText = $"Incorrect. The sign means '{CurrentSign?.CorrectAnswer}'. Take a sip!";
                FeedbackBackgroundColor = FeedbackErrorColor.ToArgbHex();
            }

            IsFeedbackVisible = true;

            Task.Delay(3000).ContinueWith(_ =>
            {
                IsFeedbackVisible = false;
                ResetButtonColors();
                LoadNextSign();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void HandleCorrectPerform()
        {
            CurrentScore++;
            FeedbackText = "Nice work! Your sign was correct!";
            FeedbackBackgroundColor = FeedbackSuccessColor.ToArgbHex();
            ShowFeedbackAndContinue();
        }

        private void HandleIncorrectPerform()
        {
            FeedbackText = "Keep practicing! Take a sip!";
            FeedbackBackgroundColor = FeedbackErrorColor.ToArgbHex();
            ShowFeedbackAndContinue();
        }

        private void ShowFeedbackAndContinue()
        {
            IsFeedbackVisible = true;
            Task.Delay(2000).ContinueWith(_ =>
            {
                IsFeedbackVisible = false;
                IsSignHidden = true;
                LoadNextSign();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void ResetGame()
        {
            CurrentScore = 0;
            _availableIndices = Enumerable.Range(0, _signs.Count).ToList();
            IsGameOver = false;
            FeedbackText = string.Empty;
            IsFeedbackVisible = false;
            ProgressPercentage = 0;
            FeedbackBackgroundColor = Colors.Transparent.ToArgbHex();
            ResetButtonColors();
            if (IsPerformMode)
            {
                IsSignHidden = true;
            }
            LoadNextSign();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}