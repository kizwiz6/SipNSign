using com.kizwiz.sipnsign.ViewModels;
using CommunityToolkit.Maui.Views;

namespace com.kizwiz.sipnsign.Pages
{
    /// <summary>
    /// Represents the game page where users guess the signs and track their scores.
    /// </summary>
    public partial class GamePage : ContentPage
    {
        private readonly GameViewModel _viewModel;
        private MediaElement? _currentVideo;
        public GameViewModel ViewModel => _viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="GamePage"/> class.
        /// </summary>
        public GamePage()
        {
            try
            {
                InitializeComponent();
                _viewModel = new GameViewModel();
                BindingContext = _viewModel;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing GamePage: {ex}");
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
        }

        /// <summary>
        /// Displays the game over screen with the final score.
        /// </summary>
        private async void ShowGameOver()
        {
            await DisplayAlert("Game Over", $"Your final score is {_viewModel.CurrentScore}. Nominate someone to drink these sips!", "OK");
            _viewModel.ResetGame(); // Reset the game in the ViewModel
        }

        private void OnModeChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.IsChecked)
            {
                _viewModel.ResetGame();
            }
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