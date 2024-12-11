using com.kizwiz.sipnsign.Enums;
using com.kizwiz.sipnsign.Services;
using com.kizwiz.sipnsign.ViewModels;
using CommunityToolkit.Maui.Views;
using System.Diagnostics;

namespace com.kizwiz.sipnsign.Pages
{
    /// <summary>
    /// Represents the game page where users guess the signs and track their scores.
    /// </summary>
    public partial class GamePage : ContentPage
    {
        private readonly GameViewModel _viewModel;
        private readonly IVideoService _videoService;
        private MediaElement? _currentVideo;

        public GameViewModel ViewModel => _viewModel;

        /// <summary>
        /// Initialises a new instance of the <see cref="GamePage"/> class.
        /// </summary>
        public GamePage(IVideoService videoService, ILoggingService logger, IProgressService progressService)
        {
            try
            {
                _videoService = videoService;
                InitializeComponent();
                _viewModel = new GameViewModel(logger, progressService);
                BindingContext = _viewModel;

                // Subscribe to sign changes
                _viewModel.PropertyChanged += async (s, e) =>
                {
                    if (e.PropertyName == nameof(GameViewModel.CurrentSign))
                    {
                        await LoadCurrentVideo();
                    }
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing GamePage: {ex}");
            }
        }

        private async Task LoadCurrentVideo()
        {
            try
            {
                if (_viewModel.CurrentSign != null)
                {
                    var videoFileName = Path.GetFileName(_viewModel.CurrentSign.VideoPath);
                    var fullPath = await _videoService.GetVideoPath(videoFileName);
                    Debug.WriteLine($"Setting video source to: {fullPath}");

                    if (IsGuessMode)
                    {
                        SignVideo.Source = MediaSource.FromFile(fullPath);
                        SignVideo.Play();
                    }
                    else
                    {
                        PerformModeVideo.Source = MediaSource.FromFile(fullPath);
                        PerformModeVideo.Play();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading video: {ex}");
            }
        }

        private bool IsGuessMode => _viewModel.CurrentMode == GameMode.Guess;

        /// <summary>
        /// Plays the video file using the injected VideoService.
        /// </summary>
        private async Task PlayVideo(string videoFileName)
        {
            try
            {
                var fullPath = await _videoService.GetVideoPath(videoFileName);
                if (_currentVideo != null)
                {
                    _currentVideo.Source = fullPath;
                    _currentVideo.Play();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing video {videoFileName}: {ex}");
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

        private void OnModeChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.IsChecked)
            {
                _viewModel.ResetGame();
            }
        }

        private void OnMediaFailed(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Media failed to load: {(sender as MediaElement)?.Source}");
        }

        private void OnMediaOpened(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Media opened successfully: {(sender as MediaElement)?.Source}");
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
