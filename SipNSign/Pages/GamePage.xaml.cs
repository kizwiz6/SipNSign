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
        private MediaElement CurrentVideoElement => IsGuessMode ? SignVideo : PerformModeVideo;

        public GameViewModel ViewModel => _viewModel;

        /// <summary>
        /// Initialises a new instance of the <see cref="GamePage"/> class.
        /// </summary>
        public GamePage(IVideoService videoService, ILoggingService logger, IProgressService progressService)
        {
            try
            {
                if (videoService == null) throw new ArgumentNullException(nameof(videoService));
                if (logger == null) throw new ArgumentNullException(nameof(logger));
                if (progressService == null) throw new ArgumentNullException(nameof(progressService));

                Debug.WriteLine("GamePage constructor started");
                InitializeComponent();
                Debug.WriteLine("InitializeComponent completed");

                _videoService = videoService;
                Debug.WriteLine("Video service assigned");

                _viewModel = new GameViewModel(videoService, logger, progressService);
                Debug.WriteLine("ViewModel created");

                BindingContext = _viewModel;
                Debug.WriteLine("BindingContext set");

                _viewModel.PropertyChanged += async (s, e) =>
                {
                    Debug.WriteLine($"PropertyChanged event fired for: {e.PropertyName}");
                    if (e.PropertyName == nameof(GameViewModel.CurrentSign))
                    {
                        Debug.WriteLine("Calling LoadCurrentVideo");
                        await LoadCurrentVideo();
                        Debug.WriteLine("LoadCurrentVideo completed");
                    }
                };
                Debug.WriteLine("GamePage constructor completed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GamePage constructor: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw the exception to be caught by the caller
            }
        }

        private async Task LoadCurrentVideo()
        {
            try
            {
                Debug.WriteLine("=== LoadCurrentVideo Start ===");

                if (_viewModel?.CurrentSign == null)
                {
                    Debug.WriteLine("CurrentSign is null");
                    return;
                }

                var videoFileName = Path.GetFileName(_viewModel.CurrentSign.VideoPath);

                if (string.IsNullOrEmpty(videoFileName))
                {
                    Debug.WriteLine("Video filename is null or empty");
                    return;
                }

                var fullPath = await _videoService.GetVideoPath(videoFileName);

                Debug.WriteLine($"Loading video: {fullPath}");
                if (!File.Exists(fullPath))
                {
                    Debug.WriteLine("Video file not found!");
                    return;
                }

                var fileInfo = new FileInfo(fullPath);
                Debug.WriteLine($"Video file size: {fileInfo.Length} bytes");

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    try
                    {
                        var mediaElement = IsGuessMode ? SignVideo : PerformModeVideo;
                        mediaElement.Source = null;

                        // Try creating a file URI
                        var uri = new Uri($"file://{fullPath}");
                        var source = MediaSource.FromUri(uri);
                        Debug.WriteLine($"Created MediaSource with URI: {uri}");

                        mediaElement.Source = source;
                        Debug.WriteLine("Set MediaSource to MediaElement");

                        mediaElement.Play();
                        Debug.WriteLine("Called Play() on MediaElement");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error playing video: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadCurrentVideo error: {ex.Message}");
            }
        }

        private async Task<string> PrepareVideoPath()
        {
            var videoFileName = Path.GetFileName(_viewModel.CurrentSign.VideoPath);
            var fullPath = await _videoService.GetVideoPath(videoFileName);
            Debug.WriteLine($"Video path: {fullPath}, Exists: {File.Exists(fullPath)}");
            return fullPath;
        }
        private void HandleGameReset()
        {
            ResetVideo();
            _viewModel.ResetGame();
            _viewModel.IsGameOver = false;
            _viewModel.IsGameActive = true;
        }

        private async Task ConfigureAndPlayVideo(MediaElement mediaElement, string fullPath)
        {
            Debug.WriteLine("Configuring video playback");
            mediaElement.ShouldAutoPlay = true;
            mediaElement.ShouldLoopPlayback = true;
            mediaElement.Volume = 1.0;

            mediaElement.Source = null;
            await Task.Delay(50);

            var source = MediaSource.FromFile(fullPath);
            mediaElement.Source = source;
            await Task.Delay(100);

            mediaElement.Play();
            Debug.WriteLine("Video playback started");
        }


        private async Task SetupAndPlayVideo(MediaElement mediaElement, MediaSource source)
        {
            try
            {
                Debug.WriteLine("Starting SetupAndPlayVideo");
                Debug.WriteLine($"MediaElement null? {mediaElement == null}");
                Debug.WriteLine($"Source null? {source == null}");

                mediaElement.Handler?.DisconnectHandler();
                Debug.WriteLine("Handler disconnected");

                mediaElement.Source = null;
                Debug.WriteLine("Source cleared");

                await Task.Delay(100);
                Debug.WriteLine("Waited after clearing source");

                mediaElement.Source = source;
                Debug.WriteLine("New source set");

                await Task.Delay(100);
                Debug.WriteLine("Waited after setting source");

                mediaElement.Play();
                Debug.WriteLine("Play called");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SetupAndPlayVideo: {ex.Message}");
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
            if (_viewModel != null)
            {
                _viewModel.IsGameOver = true;
                _viewModel.IsGameActive = false;
                StopVideo();
            }
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
            Debug.WriteLine($"Media failed to load: {(sender as MediaElement)?.Source}");
        }

        private void OnMediaOpened(object sender, EventArgs e)
        {
            Debug.WriteLine($"Media opened: {(sender as MediaElement)?.Source}");
        }

        private void OnMediaEnded(object sender, EventArgs e)
        {
            Debug.WriteLine($"Media ended: {(sender as MediaElement)?.Source}");
        }

        private void OnSeekCompleted(object sender, EventArgs e)
        {
            Debug.WriteLine($"Seek completed: {(sender as MediaElement)?.Source}");
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

        private void OnQuestionsCountChanged(object sender, ValueChangedEventArgs e)
        {
            if (sender is Slider slider)
            {
                int questions = (int)e.NewValue;
                Preferences.Set(Constants.GUESS_MODE_QUESTIONS_KEY, questions);
                // Update settings page if it exists
                MessagingCenter.Send(this, "QuestionCountChanged", questions);
            }
        }
    }
}
