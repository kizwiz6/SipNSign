using com.kizwiz.sipnsign.Models;
using System.ComponentModel;
using System.Windows.Input;

namespace com.kizwiz.sipnsign.ViewModels
{
    /// <summary>
    /// ViewModel for managing game logic and state.
    /// </summary>
    public class GameViewModel : INotifyPropertyChanged
    {
        private int _currentScore;
        private SignModel? _currentSign; // Changed to nullable
        private List<SignModel> _signs; // List of available signs
        private List<int> _availableIndices; // Indices of signs that are still available
        private string _feedbackText = string.Empty; // Initialized with an empty string
        private Color _feedbackColor; // Ensure this is assigned a default value if necessary
        private bool _isGameOver;

        public event PropertyChangedEventHandler? PropertyChanged; // Nullable event

        /// <summary>
        /// Gets or sets the current score.
        /// </summary>
        public int CurrentScore
        {
            get => _currentScore;
            set
            {
                if (_currentScore != value)
                {
                    _currentScore = value;
                    OnPropertyChanged(nameof(CurrentScore));
                }
            }
        }

        /// <summary>
        /// Gets the current sign.
        /// </summary>
        public SignModel? CurrentSign // Changed to nullable
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

        /// <summary>
        /// Gets the feedback text.
        /// </summary>
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

        /// <summary>
        /// Gets the feedback color.
        /// </summary>
        public Color FeedbackColor
        {
            get => _feedbackColor;
            set
            {
                if (_feedbackColor != value)
                {
                    _feedbackColor = value;
                    OnPropertyChanged(nameof(FeedbackColor));
                }
            }
        }

        /// <summary>
        /// Checks if the game is over.
        /// </summary>
        public bool IsGameOver
        {
            get => _isGameOver;
            set
            {
                if (_isGameOver != value)
                {
                    _isGameOver = value;
                    OnPropertyChanged(nameof(IsGameOver));
                }
            }
        }

        /// <summary>
        /// Checks if an image is available for the current sign.
        /// </summary>
        public bool IsImageAvailable => !string.IsNullOrEmpty(CurrentSign?.ImagePath);

        /// <summary>
        /// Checks if a video is available for the current sign.
        /// </summary>
        public bool IsVideoAvailable => !string.IsNullOrEmpty(CurrentSign?.VideoPath);

        /// <summary>
        /// Command to handle player's answer selection.
        /// </summary>
        public ICommand AnswerCommand => new Command<string>(CheckAndProvideFeedback);

        /// <summary>
        /// Command to restart the game.
        /// </summary>
        public ICommand PlayAgainCommand => new Command(ResetGame);

        /// <summary>
        /// Checks if the answer is correct and provides feedback.
        /// </summary>
        /// <param name="answer">The answer to check.</param>
        private void CheckAndProvideFeedback(string answer)
        {
            if (CheckAnswer(answer))
            {
                FeedbackText = "Correct!";
                FeedbackColor = Colors.Green;
                CurrentScore++;
            }
            else
            {
                FeedbackText = "Incorrect. Take a sip!";
                FeedbackColor = Colors.Red;
            }

            LoadNextSign(); // Load the next sign
        }

        /// <summary>
        /// Checks if the answer is correct.
        /// </summary>
        /// <param name="answer">The answer to check.</param>
        /// <returns>True if the answer is correct; otherwise, false.</returns>
        public bool CheckAnswer(string answer)
        {
            return answer == CurrentSign?.CorrectAnswer; // Safely access CurrentSign
        }

        /// <summary>
        /// Initializes the ViewModel and loads signs from the SignRepository.
        /// </summary>
        public GameViewModel()
        {
            SignRepository signRepository = new SignRepository(); // Create instance of SignRepository
            _signs = signRepository.GetSigns() ?? new List<SignModel>(); // Ensure _signs is initialized
            _availableIndices = new List<int>(); // Initialize to an empty list
            _feedbackColor = Colors.Transparent;
            ResetGame(); // Load the first sign
        }

        /// <summary>
        /// Loads the next sign for the player to guess.
        /// </summary>
        public void LoadNextSign()
        {
            if (_availableIndices.Count == 0) // Check if there are any available signs left
            {
                IsGameOver = true; // Set game over if there are no more signs
                return;
            }

            Random random = new Random();
            int randomIndex = random.Next(_availableIndices.Count); // Get a random index from available indices
            int selectedSignIndex = _availableIndices[randomIndex]; // Get the actual sign index
            CurrentSign = _signs[selectedSignIndex]; // Get the next sign

            // Remove the selected index from available indices
            _availableIndices.RemoveAt(randomIndex);
        }

        /// <summary>
        /// Resets the game state to start a new game.
        /// </summary>
        public void ResetGame()
        {
            CurrentScore = 0; // Reset score
            _availableIndices = Enumerable.Range(0, _signs.Count).ToList(); // Create a list of all indices
            IsGameOver = false; // Reset game over state
            FeedbackText = string.Empty; // Clear feedback text
            LoadNextSign(); // Load the first sign
        }

        /// <summary>
        /// Invokes the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
