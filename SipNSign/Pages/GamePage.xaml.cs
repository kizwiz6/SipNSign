using com.kizwiz.sipnsign.Enums;
using com.kizwiz.sipnsign.Services;
using com.kizwiz.sipnsign.ViewModels;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Core.Primitives;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;

namespace com.kizwiz.sipnsign.Pages
{
    /// <summary>
    /// Handles game interactions and video playback for both Guess and Perform modes
    /// </summary>
    public partial class GamePage : ContentPage
    {
        #region Fields
        private readonly GameViewModel _viewModel;
        private readonly IVideoService _videoService;
        private bool _isDisposed;
        private readonly SemaphoreSlim _cleanupLock = new(1, 1);
        private MediaElement? _sharedVideo;
        #endregion

        #region Properties
        public GameViewModel ViewModel => _viewModel;
        private bool IsGuessMode => _viewModel.CurrentMode == GameMode.Guess;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes game page with required services and video handling
        /// </summary>
        public GamePage(IServiceProvider serviceProvider, IVideoService videoService, ILoggingService logger, IProgressService progressService)
        {
            try
            {
                InitializeComponent();

                _videoService = videoService ?? throw new ArgumentNullException(nameof(videoService));
                _viewModel = new GameViewModel(serviceProvider, videoService, logger, progressService)
                {
                    AnswerCommand = new Command<string>(HandleAnswer),
                    RevealSignCommand = new Command(RevealSign),
                    CurrentVideoSource = MediaSource.FromFile("again.mp4")
                };
                _viewModel.SignRevealRequested += OnSignRevealRequested;

                BindingContext = _viewModel;
                ConnectToViewModel();

                _sharedVideo = this.FindByName<MediaElement>("SharedVideo") ??
                    throw new InvalidOperationException("SharedVideo element not found");

                _sharedVideo.PropertyChanged += OnSharedVideoPropertyChanged;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GamePage constructor: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region Video Handling
        /// <summary>
        /// Loads and displays video for current sign based on game mode
        /// </summary>
        private async Task LoadVideoForCurrentSign()
        {
            if (_viewModel?.CurrentSign == null) return;

            try
            {
                var videoFileName = Path.GetFileName(_viewModel.CurrentSign.VideoPath);
                Debug.WriteLine($"Attempting to load video: {videoFileName}");

#if ANDROID
                // Get the resource ID
                var context = Android.App.Application.Context;
                var resourceId = context.Resources.GetIdentifier(
                    Path.GetFileNameWithoutExtension(videoFileName).ToLower(),
                    "raw",
                    context.PackageName);

                Debug.WriteLine($"Android resource ID: {resourceId}");

                if (resourceId == 0)
                {
                    Debug.WriteLine($"Resource not found for: {videoFileName}");
                    return;
                }

                var uri = Android.Net.Uri.Parse($"android.resource://{context.PackageName}/{resourceId}");
                var source = MediaSource.FromUri(uri.ToString());
#else
                var assetPath = $"Resources/Raw/{videoFileName}";
                var stream = await FileSystem.OpenAppPackageFileAsync(videoFileName);
                var tempPath = Path.Combine(FileSystem.CacheDirectory, videoFileName);
                using (var fileStream = File.Create(tempPath))
                {
                    await stream.CopyToAsync(fileStream);
                }
                var source = MediaSource.FromFile(tempPath);
#endif

                // Handle Guess Mode
                var window = Application.Current?.Windows.FirstOrDefault();
                var gamePage = window?.Page?.Navigation?.NavigationStack.LastOrDefault() as GamePage;
                if (gamePage != null)
                {
                    gamePage.SetVideoSource(source);
                }

                // Additional handling for Perform Mode
                if (_viewModel.IsPerformMode && PerformVideo != null)
                {
                    Debug.WriteLine("Setting source for Perform Mode video");
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        try
                        {
                            PerformVideo.IsVisible = true;
                            PerformVideo.Source = source;
                            PerformVideo.ShouldAutoPlay = false; // Don't auto play in Perform Mode
                            Debug.WriteLine($"Perform Mode video source set: {source}");
                            Debug.WriteLine($"Perform Mode video visibility: {PerformVideo.IsVisible}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error setting Perform Mode video: {ex.Message}");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading video: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Sets video source and updates UI visibility
        /// </summary>
        public void SetVideoSource(MediaSource? source)
        {
            if (_isDisposed || source == null) return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    if (_sharedVideo != null)
                    {
                        _sharedVideo.Source = source;
                        _sharedVideo.IsVisible = true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error setting video source: {ex.Message}");
                }
            });
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles video property changes and logs state transitions
        /// </summary>
        private void OnSharedVideoPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not MediaElement mediaElement) return;

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

        /// <summary>
        /// Handles video open event and starts playback
        /// </summary>
        private void OnMediaOpened(object sender, EventArgs e)
        {
            Debug.WriteLine($"Media opened successfully");
            var mediaElement = sender as MediaElement;
            if (mediaElement != null)
            {
                mediaElement.Play();
            }
        }
        #endregion

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

        private void OnSignRevealRequested(object? sender, EventArgs e)
        {
            if (_isDisposed) return;

            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Debug.WriteLine("OnSignRevealRequested: Starting");
                    var performVideo = this.FindByName<MediaElement>("PerformVideo");
                    if (performVideo != null)
                    {
                        Debug.WriteLine($"PerformVideo found, Current visibility: {performVideo.IsVisible}");
                        Debug.WriteLine($"Current source: {performVideo.Source}");
                        performVideo.IsVisible = true;
                        Debug.WriteLine($"New visibility: {performVideo.IsVisible}");
                        performVideo.SeekTo(TimeSpan.Zero);
                        performVideo.Play();
                        Debug.WriteLine("Play command sent");
                    }
                    else
                    {
                        Debug.WriteLine("PerformVideo element not found");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnSignRevealRequested: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        #region Lifecycle Methods
        protected override void OnAppearing()
        {
            base.OnAppearing();
            _isDisposed = false;
        }

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
        #endregion

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

        #region Game Controls
        /// <summary>
        /// Processes user answer selection
        /// </summary>
        private void HandleAnswer(string answer)
        {
            _viewModel.HandleAnswer(answer);
        }

        /// <summary>
        /// Reveals sign video in perform mode
        /// </summary> 
        private void RevealSign()
        {
            _viewModel.RevealSign();
        }
        /// <summary>
        /// Ends current game session
        /// </summary>
        public void EndGame()
        {
            _viewModel?.EndGame();
        }
        #endregion

        private void OnQuestionsCountChanged(object sender, ValueChangedEventArgs e)
        {
            if (sender is Slider slider)
            {
                int questions = (int)e.NewValue;
                Preferences.Set(Constants.GUESS_MODE_QUESTIONS_KEY, questions);
                WeakReferenceMessenger.Default.Send(new QuestionCountChangedMessage(questions));
            }
        }

        public record QuestionCountChangedMessage(int QuestionCount);
    }
}