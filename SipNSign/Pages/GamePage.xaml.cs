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

                _viewModel.PropertyChanged += async (s, e) =>
                {
                    if (e.PropertyName == nameof(GameViewModel.CurrentSign))
                    {
                        await LoadCurrentVideo();
                    }
                    else if (e.PropertyName == nameof(GameViewModel.IsGameOver) && _viewModel.IsGameOver)
                    {
                        ResetVideo();
                    }
                    else if (e.PropertyName == nameof(GameViewModel.IsGameActive) && _viewModel.IsGameActive)
                    {
                        // Reset video when starting new game
                        ResetVideo();
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
                        // Important: Wait for UI to be ready
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            SignVideo.Stop();
                            SignVideo.Source = null;
                            await Task.Delay(50);  // Short delay for cleanup

                            // Set new source and play
                            var source = MediaSource.FromFile(fullPath);
                            SignVideo.Source = source;
                            await Task.Delay(50);  // Wait for source to be set

                            SignVideo.Play();
                            Debug.WriteLine("Video play command sent");
                        });
                    }
                    else
                    {
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            PerformModeVideo.Stop();
                            PerformModeVideo.Source = null;
                            await Task.Delay(50);

                            var source = MediaSource.FromFile(fullPath);
                            PerformModeVideo.Source = source;
                            await Task.Delay(50);

                            PerformModeVideo.Play();
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading video: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// initialize video on page appearing
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Force video reload when page appears
            if (_viewModel.CurrentSign != null)
            {
                await LoadCurrentVideo();
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

        private void StopVideo()
        {
            if (IsGuessMode)
            {
                SignVideo?.Stop();
            }
            else
            {
                PerformModeVideo?.Stop();
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

        private void OnMediaEnded(object sender, EventArgs e)
        {
            if (sender is MediaElement mediaElement)
            {
                Debug.WriteLine($"Media ended, attempting to replay");
                mediaElement.Play();
            }
        }

        private void OnMediaFailed(object sender, EventArgs e)
        {
            Debug.WriteLine($"Media failed to load: {(sender as MediaElement)?.Source}");
            if (sender is MediaElement mediaElement)
            {
                Debug.WriteLine($"Current source: {mediaElement.Source}");
            }
        }

        private void OnMediaOpened(object sender, EventArgs e)
        {
            Debug.WriteLine($"Media opened successfully: {(sender as MediaElement)?.Source}");
        }

        /// <summary>
        /// Call this method when the game ends to show the final score.
        /// </summary>
        public void EndGame()
        {
            ShowGameOver();
        }

        private void ResetVideo()
        {
            if (SignVideo != null)
            {
                SignVideo.Source = null;
                SignVideo.Stop();
            }
            if (PerformModeVideo != null)
            {
                PerformModeVideo.Source = null;
                PerformModeVideo.Stop();
            }
        }
    }
}
