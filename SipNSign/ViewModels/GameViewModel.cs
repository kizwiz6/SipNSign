using com.kizwiz.sipnsign.Models;
using System.ComponentModel;
using System.Windows.Input;

namespace com.kizwiz.sipnsign.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
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
        private Color _button1Color = Colors.Blue;
        private Color _button2Color = Colors.Blue;
        private Color _button3Color = Colors.Blue;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int CurrentScore
        {
            get => _currentScore;
            set
            {
                if (_currentScore != value)
                {
                    _currentScore = value;
                    OnPropertyChanged(nameof(CurrentScore));
                    // Update progress when score changes
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
                    OnPropertyChanged(nameof(IsImageAvailable));
                    OnPropertyChanged(nameof(IsVideoAvailable));
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

        public Color FeedbackBackgroundColor
        {
            get => Color.FromArgb(_feedbackBackgroundColor);
            set
            {
                var colorString = value.ToArgbHex();
                if (_feedbackBackgroundColor != colorString)
                {
                    _feedbackBackgroundColor = colorString;
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

        public bool IsImageAvailable => !string.IsNullOrEmpty(CurrentSign?.ImagePath);
        public bool IsVideoAvailable => !string.IsNullOrEmpty(CurrentSign?.VideoPath);

        public ICommand AnswerCommand { get; private set; }
        public ICommand PlayAgainCommand { get; private set; }

        public GameViewModel()
        {
            SignRepository signRepository = new SignRepository();
            _signs = signRepository.GetSigns() ?? new List<SignModel>();
            _availableIndices = new List<int>();
            _feedbackColor = Colors.Transparent;

            // Initialize commands
            AnswerCommand = new Command<string>(HandleAnswer);
            PlayAgainCommand = new Command(ResetGame);

            // Initialize properties
            ProgressPercentage = 0;
            IsFeedbackVisible = false;
            FeedbackBackgroundColor = Colors.Transparent;
            ResetButtonColors();

            // Start the game
            ResetGame();
        }

        private void HandleAnswer(string answer)
        {
            if (CheckAnswer(answer))
            {
                FeedbackText = "Correct!";
                FeedbackBackgroundColor = Colors.Green;
                CurrentScore++;
                UpdateButtonColor(answer, true);
            }
            else
            {
                FeedbackText = "Incorrect. Take a sip!";
                FeedbackBackgroundColor = Colors.Red;
                UpdateButtonColor(answer, false);
            }

            IsFeedbackVisible = true;

            // Hide feedback after a delay
            Task.Delay(2000).ContinueWith(_ =>
            {
                IsFeedbackVisible = false;
                ResetButtonColors();
                LoadNextSign();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void UpdateButtonColor(string answer, bool isCorrect)
        {
            Color newColor = isCorrect ? Colors.Green : Colors.Red;

            if (CurrentSign?.Choices[0] == answer) Button1Color = newColor;
            if (CurrentSign?.Choices[1] == answer) Button2Color = newColor;
            if (CurrentSign?.Choices[2] == answer) Button3Color = newColor;
        }

        private void ResetButtonColors()
        {
            Button1Color = Colors.Blue;
            Button2Color = Colors.Blue;
            Button3Color = Colors.Blue;
        }

        public bool CheckAnswer(string answer)
        {
            return answer == CurrentSign?.CorrectAnswer;
        }

        public void LoadNextSign()
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
        }

        public void ResetGame()
        {
            CurrentScore = 0;
            _availableIndices = Enumerable.Range(0, _signs.Count).ToList();
            IsGameOver = false;
            FeedbackText = string.Empty;
            IsFeedbackVisible = false;
            ProgressPercentage = 0;
            FeedbackBackgroundColor = Colors.Transparent;
            ResetButtonColors();
            LoadNextSign();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}