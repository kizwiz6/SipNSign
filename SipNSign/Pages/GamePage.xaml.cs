using com.kizwiz.sipnsign.ViewModels;

namespace com.kizwiz.sipnsign.Pages
{
    /// <summary>
    /// Represents the game page where users guess the signs and track their scores.
    /// </summary>
    public partial class GamePage : ContentPage
    {
        private readonly GameViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="GamePage"/> class.
        /// </summary>
        public GamePage()
        {
            InitializeComponent();
            _viewModel = new GameViewModel();
            BindingContext = _viewModel;

            // Start playing video if it exists
            if (SignVideo != null)
            {
                SignVideo.Play();
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.ResetGame();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (SignVideo != null)
            {
                SignVideo.Stop();
            }
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
    }
}