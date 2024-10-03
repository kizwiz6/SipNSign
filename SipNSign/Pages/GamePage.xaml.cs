using com.kizwiz.sipnsign.ViewModels;

namespace com.kizwiz.sipnsign.Pages
{
    /// <summary>
    /// Represents the game page where users guess the signs and track their scores.
    /// </summary>
    public partial class GamePage : ContentPage
    {
        private GameViewModel _viewModel; // The ViewModel that manages the game state

        /// <summary>
        /// Initializes a new instance of the <see cref="GamePage"/> class.
        /// </summary>
        public GamePage()
        {
            InitializeComponent();
            _viewModel = new GameViewModel();
            BindingContext = _viewModel; // Set the ViewModel as the binding context
            SignVideo.Play();
        }

        /// <summary>
        /// Handles the click event for answer buttons.
        /// Checks if the selected answer is correct and updates the score.
        /// Displays an alert indicating if the answer was correct or incorrect.
        /// </summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">Event arguments for the click event.</param>
        private async void OnAnswerClicked(object sender, EventArgs e)
        {
            var answerButton = (Button)sender; // Cast the sender to a Button
            var answer = answerButton.Text; // Get the answer text from the button
            bool isCorrect = _viewModel.CheckAnswer(answer); // Check if the answer is correct

            // Change the button color based on whether the answer is correct
            if (isCorrect)
            {
                answerButton.BackgroundColor = Colors.Green; // Correct answer
                answerButton.TextColor = Colors.White;        // Ensure text is visible
                await DisplayAlert("Correct!", "You got it right, nominate someone to sip!", "OK");
                _viewModel.CurrentScore++; // Increment score in the ViewModel
            }
            else
            {
                answerButton.BackgroundColor = Colors.Red; // Incorrect answer
                answerButton.TextColor = Colors.White;        // Ensure text is visible
                await DisplayAlert("Incorrect", "Wrong answer, take a drink!", "OK");
            }

            // Delay before resetting the button colors and loading the next sign
            await Task.Delay(1000); // 1 second delay

            // Reset button colors back to blue
            ResetButtonColors();

            // Load the next sign
            _viewModel.LoadNextSign();
        }

        /// <summary>
        /// Resets the button colors back to default.
        /// </summary>
        private void ResetButtonColors()
        {
            AnswerButton1.BackgroundColor = Colors.Blue;
            AnswerButton2.BackgroundColor = Colors.Blue;
            AnswerButton3.BackgroundColor = Colors.Blue;
        }

        /// <summary>
        /// Displays the game over screen with the final score.
        /// </summary>
        private async void ShowGameOver()
        {
            await DisplayAlert("Game Over", $"Your final score is {_viewModel.CurrentScore}. Nominate someone to drink these sips!", "OK");
            _viewModel.ResetGame(); // Reset the game in the ViewModel
        }

        /// <summary>
        /// Call this method when the game ends to show the final score.
        /// </summary>
        public void EndGame()
        {
            ShowGameOver();
        }

        /// <summary>
        /// Handles the click event for the Play Again button.
        /// Resets the game state to start a new game.
        /// </summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">Event arguments for the click event.</param>
        private void OnPlayAgainClicked(object sender, EventArgs e)
        {
            GameOverSection.IsVisible = false; // Hide the game over section
            _viewModel.ResetGame(); // Reset the game in the ViewModel
        }
    }
}
