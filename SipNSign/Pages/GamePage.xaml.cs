using com.kizwiz.sipnsign.Enums;
using com.kizwiz.sipnsign.Services;
using com.kizwiz.sipnsign.ViewModels;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Core.Primitives;
using System.ComponentModel;
using System.Diagnostics;

namespace com.kizwiz.sipnsign.Pages
{
    public partial class GamePage : ContentPage
    {
        private readonly GameViewModel _viewModel;
        private readonly IVideoService _videoService;
        private bool _isDisposed;
        private readonly SemaphoreSlim _cleanupLock = new(1, 1);
        private MediaElement _sharedVideo;

        public GameViewModel ViewModel => _viewModel;

        public GamePage(IServiceProvider serviceProvider, IVideoService videoService, ILoggingService logger, IProgressService progressService)
        {
            try
            {
                InitializeComponent();

                _videoService = videoService ?? throw new ArgumentNullException(nameof(videoService));
                _viewModel = new GameViewModel(serviceProvider, videoService, logger, progressService);
                _viewModel.SignRevealRequested += OnSignRevealRequested;

                BindingContext = _viewModel;
                ConnectToViewModel();

                _sharedVideo = this.FindByName<MediaElement>("SharedVideo");

                if (_sharedVideo != null)
                {
                    _sharedVideo.PropertyChanged += OnSharedVideoPropertyChanged;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GamePage constructor: {ex.Message}");
                throw;
            }
        }

        private async void OnSharedVideoPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is MediaElement mediaElement)
            {
                Debug.WriteLine($"Video property changed: {e.PropertyName}");
                if (e.PropertyName == nameof(MediaElement.CurrentState))
                {
                    Debug.WriteLine($"Current state: {mediaElement.CurrentState}");
                    if (mediaElement.CurrentState == MediaElementState.Failed)
                    {
                        Debug.WriteLine("Video failed to load");
                    }
                    else if (mediaElement.CurrentState == MediaElementState.Playing)
                    {
                        Debug.WriteLine("Video is playing");
                    }
                }
            }
        }

        private async Task LoadVideoForCurrentSign()
        {
            if (_viewModel?.CurrentSign == null) return;

            try
            {
                var videoFileName = Path.GetFileName(_viewModel.CurrentSign.VideoPath);
                Debug.WriteLine($"Attempting to load video: {videoFileName}");

#if ANDROID
                var assetPath = $"raw/{videoFileName.ToLower()}";
#else
        var assetPath = $"Resources/Raw/{videoFileName}";
#endif
                Debug.WriteLine($"Using path: {assetPath}");

                try
                {
                    using var stream = await FileSystem.OpenAppPackageFileAsync(videoFileName);
                    if (stream == null)
                    {
                        Debug.WriteLine("Failed to open video stream");
                        return;
                    }

                    Debug.WriteLine("Successfully opened video stream");

                    var tempPath = Path.Combine(FileSystem.CacheDirectory, videoFileName);
                    using (var fileStream = File.Create(tempPath))
                    {
                        await stream.CopyToAsync(fileStream);
                    }

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        try
                        {
                            var source = MediaSource.FromFile(tempPath);
                            if (_viewModel.IsGuessMode && SharedVideo != null)
                            {
                                Debug.WriteLine("Setting source for Guess Mode video");
                                SharedVideo.IsVisible = true;
                                SharedVideo.Source = source;
                                SharedVideo.ShouldAutoPlay = true;
                                SharedVideo.Play();
                            }
                            else if (_viewModel.IsPerformMode && PerformVideo != null)
                            {
                                Debug.WriteLine("Setting source for Perform Mode video");
                                PerformVideo.IsVisible = true;
                                PerformVideo.Source = source;
                                PerformVideo.ShouldAutoPlay = true;
                                PerformVideo.Play();
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error playing video: {ex.Message}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading video stream: {ex.Message}");
                    Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in LoadVideoForCurrentSign: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void OnMediaOpened(object sender, EventArgs e)
        {
            Debug.WriteLine($"Media opened successfully");
            var mediaElement = sender as MediaElement;
            if (mediaElement != null)
            {
                mediaElement.Play();
            }
        }

        private void OnMediaFailed(object sender, EventArgs e)
        {
            var mediaElement = sender as MediaElement;
            Debug.WriteLine($"Media failed to load: {mediaElement?.Source}");
            if (mediaElement?.Source is UriMediaSource uriSource)
            {
                Debug.WriteLine($"URI was: {uriSource.Uri}");
            }
        }

        private void OnMediaEnded(object sender, EventArgs e)
        {
            Debug.WriteLine($"Media playback ended: {(sender as MediaElement)?.Source}");
        }

        private async Task SafeVideoOperation(Func<Task> operation)
        {
            if (_isDisposed) return;

            try
            {
                await _cleanupLock.WaitAsync();
                if (!_isDisposed)
                {
                    await operation();
                }
            }
            catch (ObjectDisposedException)
            {
                // Ignore
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in video operation: {ex.Message}");
            }
            finally
            {
                _cleanupLock.Release();
            }
        }

        private void OnSignRevealRequested(object sender, EventArgs e)
        {
            if (_isDisposed) return;

            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (_sharedVideo != null)
                    {
                        _sharedVideo.IsVisible = true;
                        _sharedVideo.SeekTo(TimeSpan.Zero);
                        _sharedVideo.Play();
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnSignRevealRequested: {ex.Message}");
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _isDisposed = false;
        }

        public void SetVideoSource(MediaSource source)
        {
            if (_isDisposed) return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    _sharedVideo.Source = source;
                    _sharedVideo.IsVisible = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error setting video source: {ex.Message}");
                }
            });
        }

        private bool IsGuessMode => _viewModel.CurrentMode == GameMode.Guess;

        protected override void OnDisappearing()
        {
            try
            {
                _isDisposed = true;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        if (_sharedVideo != null)
                        {
                            _sharedVideo.Stop();
                            _sharedVideo.Source = null;
                            _sharedVideo.PropertyChanged -= OnSharedVideoPropertyChanged;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error cleaning up video: {ex.Message}");
                    }
                });

                _viewModel.SignRevealRequested -= OnSignRevealRequested;
            }
            finally
            {
                base.OnDisappearing();
            }
        }

        // Connect this method to the ViewModel's property changes
        private void ConnectToViewModel()
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += async (s, e) =>
                {
                    if (e.PropertyName == nameof(GameViewModel.CurrentSign))
                    {
                        Debug.WriteLine("CurrentSign changed, loading video...");
                        await LoadVideoForCurrentSign();
                    }
                };
            }
        }

        private void StopVideo()
        {
            if (_isDisposed || _sharedVideo == null) return;

            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _sharedVideo.Stop();
                    _sharedVideo.Source = null;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error stopping video: {ex.Message}");
            }
        }

        public void EndGame()
        {
            _viewModel?.EndGame();
        }

        private void OnQuestionsCountChanged(object sender, ValueChangedEventArgs e)
        {
            if (sender is Slider slider)
            {
                int questions = (int)e.NewValue;
                Preferences.Set(Constants.GUESS_MODE_QUESTIONS_KEY, questions);
                MessagingCenter.Send(this, "QuestionCountChanged", questions);
            }
        }
    }
}