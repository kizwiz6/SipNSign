using com.kizwiz.sipnsign.Models;
using System;
using System.Collections.Generic;
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
        private SignModel _currentSign; // Current sign being displayed
        private List<SignModel> _signs; // List of available signs
        private int _currentSignIndex; // Index to track the current sign in the list
        private string _feedbackText;
        private Color _feedbackColor;
        private bool _isGameOver;

        public event PropertyChangedEventHandler PropertyChanged;

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
        public SignModel CurrentSign
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
            return answer == CurrentSign.CorrectAnswer;
        }

        /// <summary>
        /// Loads the next sign for the player to guess.
        /// </summary>
        public void LoadNextSign()
        {
            // Increment the sign index and loop back if necessary
            _currentSignIndex++;
            if (_currentSignIndex >= _signs.Count)
            {
                IsGameOver = true; // Set game over if there are no more signs
                return;
            }
            CurrentSign = _signs[_currentSignIndex]; // Get the next sign
        }

        /// <summary>
        /// Initializes the ViewModel with a list of signs.
        /// </summary>
        public GameViewModel()
        {
            _signs = InitializeSigns(); // Initialize the list of signs
            _currentSignIndex = -1; // Start before the first sign
            ResetGame(); // Load the first sign
        }

        /// <summary>
        /// Resets the game state to start a new game.
        /// </summary>
        public void ResetGame()
        {
            CurrentScore = 0; // Reset score
            _currentSignIndex = -1; // Reset index to load the first sign
            IsGameOver = false; // Reset game over state
            FeedbackText = string.Empty; // Clear feedback text
            LoadNextSign(); // Load the first sign
        }

        /// <summary>
        /// Initializes the list of signs with their properties.
        /// </summary>
        /// <returns>List of SignModel</returns>
        private List<SignModel> InitializeSigns()
        {
            return new List<SignModel>
            {
                new SignModel
                {
                    VideoPath = "android.resource://com.kizwiz.sipnsign/raw/again", // No file extension needed
                    Choices = new List<string> { "Once", "Again", "Repeat" },
                    CorrectAnswer = "Again"
                },
                new SignModel
                {
                    VideoPath = "android.resource://com.kizwiz.sipnsign/raw/argue",
                    Choices = new List<string> { "Fight", "Argue", "Discuss" },
                    CorrectAnswer = "Argue"
                },
                new SignModel
                {
                    VideoPath = "android.resource://com.kizwiz.sipnsign/raw/dance",
                    Choices = new List<string> { "Jump", "Dance", "Move" },
                    CorrectAnswer = "Dance"
                },
                new SignModel
                {
                    VideoPath = "android.resource://com.kizwiz.sipnsign/raw/dog",
                    Choices = new List<string> { "Dog", "Cat", "Animal" },
                    CorrectAnswer = "Dog"
                },
                new SignModel
                {
                    VideoPath = "android.resource://com.kizwiz.sipnsign/raw/forget",
                    Choices = new List<string> { "Remember", "Forget", "Recall" },
                    CorrectAnswer = "Forget"
                },
                new SignModel
                {
                    VideoPath = "android.resource://com.kizwiz.sipnsign/raw/help",
                    Choices = new List<string> { "Assist", "Help", "Support" },
                    CorrectAnswer = "Help"
                },
                new SignModel
                {
                    VideoPath = "android.resource://com.kizwiz.sipnsign/raw/read",
                    Choices = new List<string> { "Write", "Read", "Speak" },
                    CorrectAnswer = "Read"
                },
                new SignModel
                {
                    VideoPath = "android.resource://com.kizwiz.sipnsign/raw/sun",
                    Choices = new List<string> { "Sun", "Moon", "Star" },
                    CorrectAnswer = "Sun"
                },
                // Add more signs as needed...
            };
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
