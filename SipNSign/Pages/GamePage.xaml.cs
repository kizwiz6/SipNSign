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
            var answer = ((Button)sender).Text; // Get the answer text from the button
            bool isCorrect = _viewModel.CheckAnswer(answer); // Check if the answer is correct

            if (isCorrect)
            {
                await DisplayAlert("Correct!", "You got it right, nominate someone to sip!", "OK");
            }
            else
            {
                await DisplayAlert("Incorrect", "Wrong answer, take a drink!", "OK");
            }

            _viewModel.LoadNextSign(); // Load the next sign for guessing
        }
    }
}
