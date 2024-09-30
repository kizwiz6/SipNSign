using SipNSign.Models;
using System.ComponentModel;

namespace SipNSign.ViewModels
{
    /// <summary>
    /// ViewModel for managing game logic and state.
    /// </summary>
    public class GameViewModel : INotifyPropertyChanged
    {
        private int _currentScore;
        private SignModel _currentSign; // Assuming you have a Sign model

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
        /// Checks if the answer is correct.
        /// </summary>
        /// <param name="answer">The answer to check.</param>
        /// <returns>True if the answer is correct; otherwise, false.</returns>
        public bool CheckAnswer(string answer)
        {
            // Implement your logic to check if the answer is correct
            // Example:
            return answer == _currentSign.CorrectAnswer;
        }

        /// <summary>
        /// Loads the next sign for the player to guess.
        /// </summary>
        public void LoadNextSign()
        {
            // Implement your logic to load the next sign
            // Example:
            // CurrentSign = GetNextSign();
        }

        /// <summary>
        /// Resets the game state to start a new game.
        /// </summary>
        public void ResetGame()
        {
            CurrentScore = 0; // Reset score
            LoadNextSign();   // Load the first sign
            // Optionally, reset any other game state variables
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
