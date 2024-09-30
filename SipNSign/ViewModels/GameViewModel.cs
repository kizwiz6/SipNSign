using SipNSign.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SipNSign.ViewModels
{
    /// <summary>
    /// ViewModel for managing the game logic in the SipNSign application.
    /// Implements <see cref="INotifyPropertyChanged"/> to update the UI when properties change.
    /// </summary>
    class GameViewModel : INotifyPropertyChanged
    {
        private int _currentScore; // Current score of the player
        private SignModel _currentSign; // The current sign to guess
        private List<SignModel> _signs; // List of available signs

        /// <summary>
        /// Gets or sets the current score of the player.
        /// Raises <see cref="PropertyChanged"/> event when the value changes.
        /// </summary>
        public int CurrentScore
        {
            get => _currentScore;
            set
            {
                _currentScore = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the current sign that the player needs to guess.
        /// Raises <see cref="PropertyChanged"/> event when the value changes.
        /// </summary>
        public SignModel CurrentSign
        {
            get => _currentSign;
            set
            {
                _currentSign = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the list of available signs for the game.
        /// Raises <see cref="PropertyChanged"/> event when the value changes.
        /// </summary>
        public List<SignModel> Signs
        {
            get => _signs;
            set
            {
                _signs = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameViewModel"/> class.
        /// Loads the available signs and sets the initial score.
        /// </summary>
        public GameViewModel()
        {
            LoadSigns();
            CurrentScore = 0;
        }

        /// <summary>
        /// Loads the available signs for the game into the <see cref="Signs"/> property.
        /// Sets the <see cref="CurrentSign"/> to the first sign in the list.
        /// </summary>
        public void LoadSigns()
        {
            Signs = new List<SignModel>
            {
                new SignModel { ImagePath = "sign1.png", CorrectAnswer = "Hello", Choices = new List<string> { "Hello", "Bye", "Thanks" }},
                new SignModel { ImagePath = "sign2.png", CorrectAnswer = "Drink", Choices = new List<string> { "Eat", "Drink", "Sleep" }},
            };
            CurrentSign = Signs.FirstOrDefault();
        }

        /// <summary>
        /// Checks if the provided answer is correct for the current sign.
        /// Increments the score if the answer is correct.
        /// </summary>
        /// <param name="answer">The answer provided by the user.</param>
        /// <returns><c>true</c> if the answer is correct; otherwise, <c>false</c>.</returns>
        public bool CheckAnswer(string answer)
        {
            if (answer == CurrentSign.CorrectAnswer)
            {
                CurrentScore++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Loads the next sign in the list for the player to guess.
        /// Ends the game if there are no more signs available.
        /// </summary>
        public void LoadNextSign()
        {
            int currentIndex = Signs.IndexOf(CurrentSign);
            if (currentIndex < Signs.Count - 1)
            {
                CurrentSign = Signs[currentIndex + 1];
            }
            else
            {
                // End game logic
                ShowEndGameSummary();
            }
        }

        /// <summary>
        /// Displays a summary of the final score and allows the player to restart the game.
        /// </summary>
        private async void ShowEndGameSummary()
        {
            // Here you can use any method to show a summary.
            // This example uses a simple alert.
            await Application.Current.MainPage.DisplayAlert("Game Over",
                $"Your final score is: {CurrentScore}\nDo you want to play again?",
                "Yes", "No");

            // Logic to reset the game or navigate to a different page
            ResetGame();
        }

        /// <summary>
        /// Resets the game state to allow the player to play again.
        /// </summary>
        private void ResetGame()
        {
            CurrentScore = 0;
            LoadSigns(); // Reload signs to start a new game
            CurrentSign = Signs.FirstOrDefault(); // Reset current sign
        }


        /// <summary>
        /// Event that is raised when a property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invokes the <see cref="PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
