using SipNSign.ViewModels;

namespace SipNSign.Pages
{
    /// <summary>
    /// Represents the game page where users guess the signs and track their scores.
    /// </summary>
    public partial class GamePage : ContentPage
    {
        private GameViewModel _viewModel;
        private int _score; // To track the user's score

        /// <summary>
        /// Initializes a new instance of the <see cref="GamePage"/> class.
        /// </summary>
        public GamePage()
        {
            InitializeComponent();
            _viewModel = new GameViewModel();
            BindingContext = _viewModel; // Set the ViewModel as the binding context
            _score = 0; // Initialize score
            UpdateScoreLabel(); // Update the score display
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
            var answerButton = (Button)sender;
            var answer = answerButton.Text;
            bool isCorrect = _viewModel.CheckAnswer(answer);

            // Change the button color based on whether the answer is correct
            if (isCorrect)
            {
                answerButton.BackgroundColor = Colors.Green; // Correct answer
                _score++; // Increase score for correct answer
                await DisplayAlert("Correct!", "You got it right, nominate someone to sip!", "OK");
            }
            else
            {
                answerButton.BackgroundColor = Colors.Red; // Incorrect answer
                await DisplayAlert("Incorrect", "Wrong answer, take a drink!", "OK");
            }

            // Delay before loading the next sign to allow the user to see the result
            await Task.Delay(1000); // 1 second delay

            // Reset button colors before loading the next sign
            ResetButtonColors();

            // Load the next sign
            _viewModel.LoadNextSign();

            // Update the score display
            UpdateScoreLabel();
        }

        /// <summary>
        /// Resets the button colors back to default.
        /// </summary>
        private void ResetButtonColors()
        {
            AnswerButton1.BackgroundColor = Colors.White;
            AnswerButton2.BackgroundColor = Colors.White;
            AnswerButton3.BackgroundColor = Colors.White;
        }

        /// <summary>
        /// Updates the score label to show the current score.
        /// </summary>
        private void UpdateScoreLabel()
        {
            ScoreLabel.Text = $"Score: {_score}"; // Update the score display
        }

        /// <summary>
        /// Displays the game over screen with the final score.
        /// </summary>
        private async void ShowGameOver()
        {
            await DisplayAlert("Game Over", $"Your final score is {_score}. Nominate someone to drink these sips!", "OK");
            // Optionally: Reset the score and start a new game if needed
            _score = 0;
            UpdateScoreLabel(); // Reset score display for next game
        }

        /// <summary>
        /// Call this method when the game ends to show the final score.
        /// </summary>
        public void EndGame()
        {
            ShowGameOver();
        }
    }
}
