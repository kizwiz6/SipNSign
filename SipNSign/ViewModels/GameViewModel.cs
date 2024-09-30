using SipNSign.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks; // For async/await usage

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
        private const string GameOverMessage = "Game Over";
        private const string PlayAgainMessage = "Do you want to play again?";
        private const string YesOption = "Yes";
        private const string NoOption = "No";

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
                new SignModel { ImagePath = "sign3.png", CorrectAnswer = "Eat", Choices = new List<string> { "Eat", "Drink", "Sleep" }},
                new SignModel { ImagePath = "sign4.png", CorrectAnswer = "Goodbye", Choices = new List<string> { "Goodbye", "See you", "Take care" }},
                new SignModel { ImagePath = "sign5.png", CorrectAnswer = "Thank you", Choices = new List<string> { "Thank you", "Please", "Sorry" }},
                new SignModel { ImagePath = "sign6.png", CorrectAnswer = "Welcome", Choices = new List<string> { "Welcome", "Hello", "Goodbye" }},
                new SignModel { ImagePath = "sign7.png", CorrectAnswer = "Yes", Choices = new List<string> { "Yes", "No", "Maybe" }},
                new SignModel { ImagePath = "sign8.png", CorrectAnswer = "No", Choices = new List<string> { "Yes", "No", "Okay" }},
            };

            // Randomize the signs to change their order
            RandomizeSigns();

            CurrentSign = Signs.FirstOrDefault();
        }

        /// <summary>
        /// Randomizes the order of the signs in the list.
        /// </summary>
        private void RandomizeSigns()
        {
            var random = new Random();
            Signs = Signs.OrderBy(x => random.Next()).ToList();
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
            else
            {
                ShowCorrectAnswer();
            }
            return false;
        }

        /// <summary>
        /// Displays the correct answer when the user's answer is incorrect.
        /// </summary>
        private async void ShowCorrectAnswer()
        {
            await Application.Current.MainPage.DisplayAlert("Incorrect",
                $"The correct answer was: {CurrentSign.CorrectAnswer}",
                "OK");
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
            bool playAgain = await Application.Current.MainPage.DisplayAlert(GameOverMessage,
                $"Your final score is: {CurrentScore}\n{PlayAgainMessage}",
                YesOption, NoOption);

            if (playAgain)
            {
                ResetGame();
            }
            else
            {
                // Optionally, navigate to a different page or close the app
            }
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
