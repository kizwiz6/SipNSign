using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.ComponentModel;
using System.Windows.Input;
using com.kizwiz.sipnsign.Models;
using com.kizwiz.sipnsign.Enums;

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
        private const int QuestionTimeLimit = 10;
        private int _remainingTime;
        private bool _isLoading;
        private int _currentScore;
        private SignModel? _currentSign;
        private List<SignModel> _signs;
        private List<int> _availableIndices;
        private string _feedbackText = string.Empty;
        private bool _isGameOver;
        private double _progressPercentage;
        private string _feedbackBackgroundColor;
        private bool _isFeedbackVisible;
        private int _finalScore;
        private Color _button1Color;
        private Color _button2Color;
        private Color _button3Color;
        private Color _button4Color;
        private GameMode _currentMode = GameMode.Guess;
        private bool _isSignHidden = true;
        #endregion

        #region Public Properties
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

        public Color Button4Color
        {
            get => _button4Color;
            set
            {
                if (_button4Color != value)
                {
                    _button4Color = value;
                    OnPropertyChanged(nameof(Button4Color));
                }
            }
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
        public ICommand PlayAgainCommand { get; private set; }
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
        public GameViewModel()
        {
            try
            {
                SignRepository signRepository = new SignRepository();
                _signs = signRepository.GetSigns();
                if (!_signs.Any())
                {
                    System.Diagnostics.Debug.WriteLine("No signs loaded");
                    return;
                }

                foreach (var sign in _signs)
                {
                    System.Diagnostics.Debug.WriteLine($"Video path: {sign.VideoPath}");
                }

                _availableIndices = new List<int>();
                _feedbackBackgroundColor = Colors.Transparent.ToArgbHex();

                _timer = Application.Current.Dispatcher.CreateTimer();
                _timer.Interval = TimeSpan.FromSeconds(1);
                _timer.Tick += Timer_Tick;

                InitializeCommands();
                ResetButtonColors();
                InitializeGame();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GameViewModel: {ex}");
            }
            
            
        }
        #endregion

        #region Private Methods
        private void InitializeCommands()
        {
            AnswerCommand = new Command<string>(HandleAnswer);
            PlayAgainCommand = new Command(ResetGame);
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

        private void HandleTimeOut()
        {
            FeedbackText = $"Time's up! The sign means '{CurrentSign?.CorrectAnswer}'. Take a sip!";
            FeedbackBackgroundColor = FeedbackErrorColor.ToArgbHex();
            IsFeedbackVisible = true;

            Task.Delay(3000).ContinueWith(_ =>
            {
                IsFeedbackVisible = false;
                LoadNextSign();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private bool CheckAnswer(string answer)
        {
            return answer == CurrentSign?.CorrectAnswer;
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

            ShowFeedbackAndContinue();
        }

        private void RevealSign()
        {
            IsSignHidden = false;
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
                if (IsPerformMode) IsSignHidden = true;
                LoadNextSign();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void UpdateButtonColor(string answer, bool isCorrect)
        {
            Color newColor = isCorrect ? FeedbackSuccessColor : FeedbackErrorColor;

            if (CurrentSign?.Choices[0] == answer) Button1Color = newColor;
            if (CurrentSign?.Choices[1] == answer) Button2Color = newColor;
            if (CurrentSign?.Choices[2] == answer) Button3Color = newColor;
            if (CurrentSign?.Choices[3] == answer) Button4Color = newColor;
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
            RemainingTime = QuestionTimeLimit;
            _timer.Start();
        }

        private void SwitchMode(GameMode mode)
        {
            CurrentMode = mode;
        }
        #endregion

        #region Public Methods
        public void LoadNextSign()
        {
            IsLoading = true;

            try
            {
                if (_availableIndices.Count == 0)
                {
                    IsGameOver = true;
                    return;
                }

                Random random = new Random();
                int randomIndex = random.Next(_availableIndices.Count);
                int selectedSignIndex = _availableIndices[randomIndex];
                CurrentSign = _signs[selectedSignIndex];
                _availableIndices.RemoveAt(randomIndex);

                if (IsGuessMode)
                {
                    StartTimer();
                }
            }
            finally
            {
                IsLoading = false;
            }
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
        #endregion
    }
}