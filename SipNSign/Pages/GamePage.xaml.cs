using SipNSign.ViewModels;

namespace SipNSign.Pages
{
    /// <summary>
    /// Represents the game page where users guess the signs and track their scores.
    /// </summary>
    public partial class GamePage : ContentPage
    {
        private GameViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="GamePage"/> class.
        /// </summary>
        public GamePage()
        {
            InitializeComponent();
            _viewModel = new GameViewModel();
            BindingContext = _viewModel; // Set the ViewModel as the binding context
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
    }
}
