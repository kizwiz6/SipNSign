using com.kizwiz.sipnsign.Enums;
using com.kizwiz.sipnsign.Services;
using com.kizwiz.sipnsign.ViewModels;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Core.Primitives;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using com.kizwiz.sipnsign.Converters;
using com.kizwiz.sipnsign.Models;

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
        private readonly SemaphoreSlim _videoLoadLock = new SemaphoreSlim(1, 1);
        private bool _isDisposed;
        private readonly SemaphoreSlim _cleanupLock = new(1, 1);
        private IDispatcherTimer? _timer;
        private MediaElement? _multiplayerGuessVideoElement;
        #endregion

        #region Properties
        public GameViewModel ViewModel => (GameViewModel)BindingContext;
        private bool IsGuessMode => _viewModel.CurrentMode == GameMode.Guess;

        // Add properties for the MediaElements with distinct names to avoid conflicts
        // with the XAML-generated fields
        private MediaElement? _sharedVideoElement;
        private MediaElement? _performVideoElement;
        private MediaElement? _multiplayerPerformVideoElement;
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
                BindingContext = _viewModel;
                _viewModel = new GameViewModel(serviceProvider, videoService, logger, progressService)
                {
                    AnswerCommand = new Command<string>(HandleAnswer),
                    RevealSignCommand = new Command(RevealSign),
                    CurrentVideoSource = MediaSource.FromFile("again.mp4")
                };
                _viewModel.SignRevealRequested += OnSignRevealRequested;

                BindingContext = _viewModel;
                ConnectToViewModel();

                this.Loaded += OnPageLoaded;

                this.Loaded += (s, e) => {
                    Debug.WriteLine($"=== GamePage Loaded ===");
                    Debug.WriteLine($"IsGuessMode: {_viewModel.IsGuessMode}");
                    Debug.WriteLine($"IsMultiplayer: {_viewModel.IsMultiplayer}");
                    Debug.WriteLine($"Players count: {_viewModel.Players.Count}");
                };
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
            if (_isDisposed || _viewModel?.CurrentSign == null) return;

            try
            {
                await _videoLoadLock.WaitAsync();

                var videoFileName = Path.GetFileName(_viewModel.CurrentSign.VideoPath);
                Debug.WriteLine($"=== LoadVideoForCurrentSign: {videoFileName} ===");
                Debug.WriteLine($"Mode: {_viewModel.CurrentMode}, Multiplayer: {_viewModel.IsMultiplayer}");

                // Get the video URI
                var videoPath = await _videoService.GetVideoPath(videoFileName);
                var source = MediaSource.FromUri(videoPath);
                Debug.WriteLine($"Video source created: {videoPath}");

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    try
                    {
                        MediaElement? targetElement = null;
                        string elementName = "";

                        // Determine which element to use
                        if (_viewModel.IsGuessMode && _viewModel.IsMultiplayer)
                        {
                            targetElement = _multiplayerGuessVideoElement;
                            elementName = "MultiplayerGuessVideo";
                        }
                        else if (_viewModel.IsGuessMode && !_viewModel.IsMultiplayer)
                        {
                            targetElement = _sharedVideoElement;
                            elementName = "SharedVideo";
                        }
                        else if (_viewModel.IsPerformMode && _viewModel.IsMultiplayer)
                        {
                            targetElement = _multiplayerPerformVideoElement;
                            elementName = "MultiplayerPerformVideo";
                        }
                        else if (_viewModel.IsPerformMode && !_viewModel.IsMultiplayer)
                        {
                            targetElement = _performVideoElement;
                            elementName = "PerformVideo";
                        }

                        Debug.WriteLine($"Target element: {elementName}, Exists: {targetElement != null}");

                        if (targetElement != null && !_isDisposed)
                        {
                            // Stop any existing playback
                            try { targetElement.Stop(); } catch { }

                            // Set new source
                            targetElement.Source = source;
                            targetElement.IsVisible = true;

                            // Auto-play for Guess mode, manual for Perform mode
                            targetElement.ShouldAutoPlay = _viewModel.IsGuessMode;

                            if (_viewModel.IsGuessMode)
                            {
                                targetElement.Play();
                            }

                            Debug.WriteLine($"Video loaded on {elementName}");
                        }
                        else
                        {
                            Debug.WriteLine($"ERROR: Target element is null or disposed!");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"ERROR setting video source: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR in LoadVideoForCurrentSign: {ex.Message}");
            }
            finally
            {
                _videoLoadLock.Release();
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
                    if (_sharedVideoElement != null)
                    {
                        _sharedVideoElement.Stop();
                        _sharedVideoElement.Source = null;
                        _sharedVideoElement.Source = source;
                        _sharedVideoElement.IsVisible = true;
                        _sharedVideoElement.SeekTo(TimeSpan.Zero);
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
        /// Handles video open event and starts playback with audio disabled
        /// </summary>
        private void OnMediaOpened(object sender, EventArgs e)
        {
            Debug.WriteLine($"Media opened successfully");
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var mediaElement = sender as MediaElement;
                if (mediaElement != null)
                {
                    try
                    {
                        // Log detailed media information
                        Debug.WriteLine($"=== MEDIA DETAILS ===");
                        Debug.WriteLine($"Source: {mediaElement.Source}");
                        Debug.WriteLine($"Duration: {mediaElement.Duration}");
                        Debug.WriteLine($"Current State: {mediaElement.CurrentState}");
                        Debug.WriteLine($"Volume: {mediaElement.Volume}");
                        Debug.WriteLine($"Aspect: {mediaElement.Aspect}");
                        Debug.WriteLine($"Width Request: {mediaElement.WidthRequest}");
                        Debug.WriteLine($"Height Request: {mediaElement.HeightRequest}");

                        // Ensure audio is completely disabled
                        mediaElement.Volume = 0;
                        mediaElement.ShouldMute = true;

                        Debug.WriteLine($"Audio disabled for MediaElement: Volume={mediaElement.Volume}, Muted={mediaElement.ShouldMute}");

                        mediaElement.Play();
                        Debug.WriteLine($"Play command sent, Current State: {mediaElement.CurrentState}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"ERROR in OnMediaOpened: {ex.Message}");
                        Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    }
                }
            });
        }

        private async void OnConfirmGuessClicked(object sender, EventArgs e)
        {
            if (BindingContext is not ViewModels.GameViewModel vm)
                return;

            // If multiplayer and not everyone answered, show a popup with the missing players
            if (vm.IsMultiplayer && !vm.HasAllPlayersAnswered)
            {
                var unanswered = vm.Players
                                  .Where(p => !p.HasAnswered)
                                  .Select(p => p.Name)
                                  .ToList();

                string names = unanswered.Any() ? string.Join(", ", unanswered) : "No one";
                string plural = unanswered.Count > 1 ? "are" : "is";
                await DisplayAlert("Waiting for Players", $"{names} {plural} still to answer.", "OK");
                return;
            }

            // All answered -> execute the Confirm command via ViewModel
            if (vm.ConfirmGuessAnswersCommand?.CanExecute(null) ?? false)
            {
                vm.ConfirmGuessAnswersCommand.Execute(null);
            }
        }
        #endregion

        private void OnMediaFailed(object sender, EventArgs e)
        {
            var mediaElement = sender as MediaElement;
            Debug.WriteLine($"=== MEDIA FAILED ===");
            Debug.WriteLine($"Source: {mediaElement?.Source}");

            if (mediaElement?.Source is UriMediaSource uriSource)
            {
                Debug.WriteLine($"URI: {uriSource.Uri}");

                // Check if file exists and get file info
                try
                {
                    var uri = uriSource.Uri.ToString();
                    if (uri.StartsWith("android.resource://"))
                    {
                        Debug.WriteLine("Android resource - checking resource ID");
                    }
                    else if (uri.StartsWith("file://"))
                    {
                        var filePath = uri.Replace("file://", "");
                        if (File.Exists(filePath))
                        {
                            var fileInfo = new FileInfo(filePath);
                            Debug.WriteLine($"File size: {fileInfo.Length} bytes ({fileInfo.Length / 1024.0:F2} KB)");
                            Debug.WriteLine($"File extension: {fileInfo.Extension}");
                        }
                        else
                        {
                            Debug.WriteLine($"File does not exist: {filePath}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error checking file info: {ex.Message}");
                }
            }
        }

        private void OnMediaEnded(object sender, EventArgs e)
        {
            Debug.WriteLine($"Media playback ended: {(sender as MediaElement)?.Source}");
        }

        private void OnPageLoaded(object sender, EventArgs e)
        {
            try
            {
                // Only initialize MediaElements for current mode
                if (ViewModel.IsGuessMode)
                {
                    if (ViewModel.IsMultiplayer)
                    {
                        _multiplayerGuessVideoElement = FindMediaElement("MultiplayerGuessVideo");
                        Debug.WriteLine($"MultiplayerGuessVideo element found: {_multiplayerGuessVideoElement != null}");
                    }
                    else
                    {
                        _sharedVideoElement = FindMediaElement("SharedVideo");
                        Debug.WriteLine($"SharedVideo element found: {_sharedVideoElement != null}");
                    }
                }
                else if (ViewModel.IsPerformMode)
                {
                    if (ViewModel.IsMultiplayer)
                    {
                        _multiplayerPerformVideoElement = FindMediaElement("MultiplayerPerformVideo");
                    }
                    else
                    {
                        _performVideoElement = FindMediaElement("PerformVideo");
                    }
                }

                // Set up event handlers
                if (_sharedVideoElement != null)
                {
                    _sharedVideoElement.PropertyChanged += OnSharedVideoPropertyChanged;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnPageLoaded: {ex.Message}");
            }
        }

        private void InitializeMediaElements()
        {
            try
            {
                // Use safer element finding with null checks
                _sharedVideoElement = FindMediaElement("SharedVideo");
                _performVideoElement = FindMediaElement("PerformVideo");
                _multiplayerPerformVideoElement = FindMediaElement("MultiplayerPerformVideo");

                // Set up event handlers only for successfully found elements
                if (_sharedVideoElement != null)
                {
                    _sharedVideoElement.PropertyChanged += OnSharedVideoPropertyChanged;
                    Debug.WriteLine("SharedVideo element initialized successfully");
                }
                else
                {
                    Debug.WriteLine("WARNING: SharedVideo element not found!");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in InitializeMediaElements: {ex.Message}");
            }
        }

        private MediaElement? FindMediaElement(string name)
        {
            try
            {
                // More robust element finding
                var element = this.FindByName(name);
                if (element is MediaElement mediaElement)
                {
                    return mediaElement;
                }

                Debug.WriteLine($"Element '{name}' found but is not a MediaElement (type: {element?.GetType().Name ?? "null"})");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error finding MediaElement '{name}': {ex.Message}");
                return null;
            }
        }

        private void OnSignRevealRequested(object? sender, EventArgs e)
        {
            if (_isDisposed) return;

            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Debug.WriteLine("OnSignRevealRequested: Starting");

                    // Check which mode we're in (single or multiplayer)
                    bool isMultiplayer = _viewModel?.IsMultiplayer ?? false;

                    // Get the appropriate video element
                    MediaElement? videoToPlay = null;

                    if (isMultiplayer)
                    {
                        // Use multiplayer video
                        videoToPlay = _multiplayerPerformVideoElement ?? this.FindByName<MediaElement>("MultiplayerPerformVideo");
                        Debug.WriteLine("Using MultiplayerPerformVideo");
                    }
                    else
                    {
                        // Use single player video
                        videoToPlay = _performVideoElement ?? this.FindByName<MediaElement>("PerformVideo");
                        Debug.WriteLine("Using PerformVideo");
                    }

                    if (videoToPlay != null)
                    {
                        Debug.WriteLine($"Video element found, Current visibility: {videoToPlay.IsVisible}");
                        Debug.WriteLine($"Current source: {videoToPlay.Source}");
                        videoToPlay.IsVisible = true;
                        Debug.WriteLine($"New visibility: {videoToPlay.IsVisible}");
                        videoToPlay.SeekTo(TimeSpan.Zero);
                        videoToPlay.Play();
                        Debug.WriteLine("Play command sent");
                    }
                    else
                    {
                        Debug.WriteLine("Video element not found");
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

            // Disable overscroll on Android to prevent video blank bug
#if ANDROID
            DisableScrollViewOverscroll();
#endif

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_sharedVideoElement != null)
                {
                    _sharedVideoElement.Stop();
                    _sharedVideoElement.Source = null;
                }
            });

            if (ViewModel.IsMultiplayer && ViewModel.IsPerformMode)
            {
                TestPlayerConverter();
            }
        }

#if ANDROID
        /// <summary>
        /// Disables overscroll effect on Android ScrollView to prevent video blanking bug
        /// </summary>
        private void DisableScrollViewOverscroll()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    var scrollView = this.FindByName<ScrollView>("MultiplayerScrollView");
                    if (scrollView == null)
                    {
                        Debug.WriteLine("MultiplayerScrollView not found");
                        return;
                    }

                    // Wait for handler to be initialized with retry logic
                    int retries = 0;
                    while (scrollView.Handler?.PlatformView == null && retries < 10)
                    {
                        await Task.Delay(50);
                        retries++;
                    }

                    if (scrollView.Handler?.PlatformView is Android.Views.View androidView)
                    {
                        Debug.WriteLine("Found MultiplayerScrollView platform view");

                        // Disable overscroll mode on the view itself
                        androidView.OverScrollMode = Android.Views.OverScrollMode.Never;
                        Debug.WriteLine("Set OverScrollMode.Never on view");

                        // Find and disable on the native ScrollView parent
                        var parent = androidView.Parent;
                        while (parent != null)
                        {
                            if (parent is Android.Widget.ScrollView androidScrollView)
                            {
                                androidScrollView.OverScrollMode = Android.Views.OverScrollMode.Never;
                                Debug.WriteLine("Set OverScrollMode.Never on Android.Widget.ScrollView");
                                break;
                            }
                            parent = parent.Parent;
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"MultiplayerScrollView platform view not ready after {retries} retries");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error disabling overscroll: {ex.Message}");
                }
            });
        }
#endif

        protected override async void OnDisappearing()
        {
            try
            {
                _isDisposed = true;

                // Call Cleanup explicitly first
                Cleanup();

                // Then proceed with existing code if needed
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (_sharedVideoElement != null)
                    {
                        _sharedVideoElement.Stop();
                        _sharedVideoElement.Source = null;
                        _sharedVideoElement.Handler?.DisconnectHandler();
                    }

                    if (_performVideoElement != null)
                    {
                        _performVideoElement.Stop();
                        _performVideoElement.Source = null;
                        _performVideoElement.Handler?.DisconnectHandler();
                    }

                    if (_multiplayerPerformVideoElement != null)
                    {
                        _multiplayerPerformVideoElement.Stop();
                        _multiplayerPerformVideoElement.Source = null;
                        _multiplayerPerformVideoElement.Handler?.DisconnectHandler();
                    }
                });

                await Task.Delay(100);

                ViewModel?.Cleanup();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnDisappearing: {ex.Message}");
            }
            finally
            {
                base.OnDisappearing();
                _isDisposed = false;
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
                        Debug.WriteLine($"CurrentSign changed to: {_viewModel.CurrentSign?.CorrectAnswer}");

                        // Initialize elements if not done yet
                        if (_viewModel.IsGuessMode && _viewModel.IsMultiplayer && _multiplayerGuessVideoElement == null)
                        {
                            Debug.WriteLine("MultiplayerGuessVideo is null, initializing...");
                            InitializeVideoElementsForCurrentMode();
                        }
                        else if (_viewModel.IsGuessMode && !_viewModel.IsMultiplayer && _sharedVideoElement == null)
                        {
                            Debug.WriteLine("SharedVideo is null, initializing...");
                            InitializeVideoElementsForCurrentMode();
                        }
                        else
                        {
                            await LoadVideoForCurrentSign();
                        }
                    }

                    if (e.PropertyName == nameof(GameViewModel.IsMultiplayer))
                    {
                        Debug.WriteLine($"IsMultiplayer changed to: {_viewModel.IsMultiplayer}");
                        // Reinitialize to find the right video element
                        InitializeVideoElementsForCurrentMode();
                    }
                };
            }
        }

        private async void InitializeVideoElementsForCurrentMode()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    Debug.WriteLine($"=== InitializeVideoElementsForCurrentMode ===");
                    Debug.WriteLine($"IsGuessMode: {_viewModel.IsGuessMode}");
                    Debug.WriteLine($"IsMultiplayer: {_viewModel.IsMultiplayer}");

                    // Clear all previous elements
                    _sharedVideoElement = null;
                    _performVideoElement = null;
                    _multiplayerPerformVideoElement = null;
                    _multiplayerGuessVideoElement = null;

                    // CRITICAL: Wait for UI to update with IsMultiplayer binding
                    await Task.Delay(200);

                    // Find and set the correct element for current mode
                    if (_viewModel.IsGuessMode)
                    {
                        if (_viewModel.IsMultiplayer)
                        {
                            _multiplayerGuessVideoElement = this.FindByName<MediaElement>("MultiplayerGuessVideo");
                            Debug.WriteLine($"Initialized MultiplayerGuessVideo: {_multiplayerGuessVideoElement != null}");

                            if (_multiplayerGuessVideoElement == null)
                            {
                                // Try again after another delay
                                await Task.Delay(300);
                                _multiplayerGuessVideoElement = this.FindByName<MediaElement>("MultiplayerGuessVideo");
                                Debug.WriteLine($"Retry MultiplayerGuessVideo: {_multiplayerGuessVideoElement != null}");
                            }
                        }
                        else
                        {
                            _sharedVideoElement = this.FindByName<MediaElement>("SharedVideo");
                            Debug.WriteLine($"Initialized SharedVideo: {_sharedVideoElement != null}");
                        }
                    }
                    else if (_viewModel.IsPerformMode)
                    {
                        if (_viewModel.IsMultiplayer)
                        {
                            _multiplayerPerformVideoElement = this.FindByName<MediaElement>("MultiplayerPerformVideo");
                            Debug.WriteLine($"Initialized MultiplayerPerformVideo: {_multiplayerPerformVideoElement != null}");
                        }
                        else
                        {
                            _performVideoElement = this.FindByName<MediaElement>("PerformVideo");
                            Debug.WriteLine($"Initialized PerformVideo: {_performVideoElement != null}");
                        }
                    }

                    // If we have a current sign and found the element, load the video
                    if (_viewModel.CurrentSign != null)
                    {
                        Debug.WriteLine("Element found and CurrentSign exists, loading video...");
                        await LoadVideoForCurrentSign();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error initializing video elements: {ex.Message}");
                }
            });
        }

        private async Task CleanupMediaElements()
        {
            try
            {
                await _videoLoadLock.WaitAsync();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (_sharedVideoElement != null)
                    {
                        _sharedVideoElement.Stop();
                        _sharedVideoElement.Source = null;
                        _sharedVideoElement.PropertyChanged -= OnSharedVideoPropertyChanged;
                        _sharedVideoElement.Handler?.DisconnectHandler();
                    }

                    if (_performVideoElement != null)
                    {
                        _performVideoElement.Stop();
                        _performVideoElement.Source = null;
                        _performVideoElement.Handler?.DisconnectHandler();
                    }

                    if (_multiplayerPerformVideoElement != null)
                    {
                        _multiplayerPerformVideoElement.Stop();
                        _multiplayerPerformVideoElement.Source = null;
                        _multiplayerPerformVideoElement.Handler?.DisconnectHandler();
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cleaning up media elements: {ex.Message}");
            }
            finally
            {
                _sharedVideoElement = null;
                _performVideoElement = null;
                _multiplayerPerformVideoElement = null;
                _videoLoadLock.Release();
            }
        }

        public async void Cleanup()
        {
            if (_isDisposed) return;

            try
            {
                Debug.WriteLine("Cleanup: Starting");
                _isDisposed = true;

                if (_timer != null)
                {
                    _timer.Stop();
                    _timer = null;
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    try
                    {
                        _sharedVideoElement?.Handler?.DisconnectHandler();
                        _performVideoElement?.Handler?.DisconnectHandler();
                        _multiplayerPerformVideoElement?.Handler?.DisconnectHandler();
                        _multiplayerGuessVideoElement?.Handler?.DisconnectHandler(); // ADD THIS
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error disconnecting handlers: {ex.Message}");
                    }
                });

                _sharedVideoElement = null;
                _performVideoElement = null;
                _multiplayerPerformVideoElement = null;
                _multiplayerGuessVideoElement = null; // ADD THIS

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                Debug.WriteLine("Cleanup: Complete");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in Cleanup: {ex.Message}");
            }
        }

        public void TestPlayerConverter()
        {
            Debug.WriteLine("=== TESTING PLAYER CONVERTER ===");
            var converter = Resources["PlayerAnswerConverter"] as PlayerAnswerConverter;

            if (converter == null)
            {
                Debug.WriteLine("ERROR: PlayerAnswerConverter not found in Resources");
                return;
            }

            Debug.WriteLine("PlayerAnswerConverter found in Resources");

            // Test with a dummy player
            var player = new Player { Name = "Test Player" };
            var result = converter.Convert(player, typeof(PlayerAnswerParameter), true, null);

            if (result is PlayerAnswerParameter param)
            {
                Debug.WriteLine($"Converter works! Player: {param.Player.Name}, IsCorrect: {param.IsCorrect}");
            }
            else
            {
                Debug.WriteLine("ERROR: Converter did not return a PlayerAnswerParameter");
            }
        }

        /// <summary>
        /// Forces garbage collection to help prevent memory-related crashes
        /// </summary>
        private void ForceGarbageCollection()
        {
            try
            {
                Debug.WriteLine("Forcing garbage collection...");
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Debug.WriteLine("Garbage collection completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during garbage collection: {ex.Message}");
            }
        }


        private void StopVideo()
        {
            if (_isDisposed || _sharedVideoElement == null) return;

            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _sharedVideoElement.Stop();
                    _sharedVideoElement.Source = null;
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
            try
            {
                if (ViewModel != null)
                {
                    ViewModel.EndGame();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error ending game: {ex.Message}");
            }
        }
        #endregion

        private void DisableAudioOnAllVideos()
        {
            try
            {
                if (_sharedVideoElement != null)
                {
                    _sharedVideoElement.Volume = 0;
                    _sharedVideoElement.ShouldMute = true;
                    Debug.WriteLine("Audio disabled on SharedVideo");
                }

                if (_performVideoElement != null)
                {
                    _performVideoElement.Volume = 0;
                    _performVideoElement.ShouldMute = true;
                    Debug.WriteLine("Audio disabled on PerformVideo");
                }

                if (_multiplayerPerformVideoElement != null)
                {
                    _multiplayerPerformVideoElement.Volume = 0;
                    _multiplayerPerformVideoElement.ShouldMute = true;
                    Debug.WriteLine("Audio disabled on MultiplayerPerformVideo");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error disabling audio: {ex.Message}");
            }
        }

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

        /// <summary>
        /// Handles clicking the correct (check mark) button for a player
        /// </summary>
        private void OnPlayerCorrectClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && !string.IsNullOrEmpty(button.ClassId))
                {
                    string playerName = button.ClassId;
                    Debug.WriteLine($"OnPlayerCorrectClicked: Player name = {playerName}");

                    // Find the player by name
                    var player = _viewModel.Players.FirstOrDefault(p => p.Name == playerName);
                    if (player != null)
                    {
                        Debug.WriteLine($"Found player: {player.Name}, recording correct answer");

                        // Use the new RecordAnswer method (allows changing answers)
                        player.RecordAnswer(true);

                        // Notify the ViewModel that the player's answer state has changed
                        ViewModel.RecordPlayerAnswer(new PlayerAnswerParameter
                        {
                            Player = player,
                            IsCorrect = true
                        });

                        // Show feedback
                        _viewModel.FeedbackText = $"{player.Name} got it right!";
                        _viewModel.FeedbackBackgroundColor = _viewModel.GetFeedbackColor(true);
                        _viewModel.IsFeedbackVisible = true;

                        // Force UI refresh
                        _viewModel.OnPropertyChanged(nameof(_viewModel.Players));
                        _viewModel.OnPropertyChanged(nameof(_viewModel.HasAllPlayersAnswered));

                        Debug.WriteLine($"Player {player.Name} updated: HasAnswered={player.HasAnswered}, GotCurrentAnswerCorrect={player.GotCurrentAnswerCorrect}, Score={player.Score}");
                        Debug.WriteLine($"HasAllPlayersAnswered: {_viewModel.HasAllPlayersAnswered}");

                        // Auto-hide feedback after 2 seconds
                        Device.StartTimer(TimeSpan.FromSeconds(2), () => {
                            _viewModel.IsFeedbackVisible = false;
                            return false; // Don't repeat
                        });
                    }
                    else
                    {
                        Debug.WriteLine($"Player not found with name: {playerName}");
                    }
                }
                else
                {
                    Debug.WriteLine("Button sender is null or ClassId is empty");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnPlayerCorrectClicked: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles clicking the incorrect (X mark) button for a player
        /// </summary>
        private void OnPlayerIncorrectClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && !string.IsNullOrEmpty(button.ClassId))
                {
                    string playerName = button.ClassId;
                    Debug.WriteLine($"OnPlayerIncorrectClicked: Player name = {playerName}");

                    // Find the player by name
                    var player = _viewModel.Players.FirstOrDefault(p => p.Name == playerName);
                    if (player != null)
                    {
                        Debug.WriteLine($"Found player: {player.Name}, recording incorrect answer");

                        // Use the new RecordAnswer method (allows changing answers)
                        player.RecordAnswer(false);

                        ViewModel.RecordPlayerAnswer(new PlayerAnswerParameter
                        {
                            Player = player,
                            IsCorrect = false
                        });

                        // Show feedback
                        _viewModel.FeedbackText = $"{player.Name} got it wrong!";
                        _viewModel.FeedbackBackgroundColor = _viewModel.GetFeedbackColor(false);
                        _viewModel.IsFeedbackVisible = true;

                        // Force UI refresh
                        _viewModel.OnPropertyChanged(nameof(_viewModel.Players));
                        _viewModel.OnPropertyChanged(nameof(_viewModel.HasAllPlayersAnswered));

                        Debug.WriteLine($"Player {player.Name} updated: HasAnswered={player.HasAnswered}, GotCurrentAnswerCorrect={player.GotCurrentAnswerCorrect}, Score={player.Score}");
                        Debug.WriteLine($"HasAllPlayersAnswered: {_viewModel.HasAllPlayersAnswered}");

                        // Auto-hide feedback after 2 seconds
                        Device.StartTimer(TimeSpan.FromSeconds(2), () => {
                            _viewModel.IsFeedbackVisible = false;
                            return false; // Don't repeat
                        });
                    }
                    else
                    {
                        Debug.WriteLine($"Player not found with name: {playerName}");
                    }
                }
                else
                {
                    Debug.WriteLine("Button sender is null or ClassId is empty");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnPlayerIncorrectClicked: {ex.Message}");
            }
        }

        private void OnPlayerAnswerClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var player = button.CommandParameter as Player;
            var answer = int.Parse(button.Text);

            if (player != null)
            {
                player.SelectedAnswer = answer;
                // Update UI to reflect selected answer, if needed
                _viewModel.OnPropertyChanged(nameof(_viewModel.HasAllPlayersAnswered));
            }
        }

        private async void OnConfirmAnswersClicked(object sender, EventArgs e)
        {
            if (ViewModel.IsMultiplayer && !ViewModel.HasAllPlayersAnswered)
            {
                var unansweredPlayers = ViewModel.Players.Where(p => p.SelectedAnswer == 0).ToList();
                var playerNames = string.Join(", ", unansweredPlayers.Select(p => p.Name));

                await Application.Current.MainPage.DisplayAlert(
                    "Waiting for Players",
                    $"Still waiting for: {playerNames}\n\nMake sure all players have selected their answers (1-4).",
                    "OK");
                return;
            }

            bool allPlayersCorrect = true;

            foreach (var player in ViewModel.Players)
            {
                var isCorrect = player.SelectedAnswer == ViewModel.CurrentAnswer;
                player.Score += isCorrect ? 1 : 0;

                if (!isCorrect)
                {
                    allPlayersCorrect = false;
                }
            }

            // Show feedback and move to next question
            ViewModel.ShowFeedbackAndContinue(allPlayersCorrect);
        }

        /// <summary>
        /// Handles when a player selects an answer (1-4) in multiplayer Guess mode
        /// </summary>
        private void OnPlayerGuessAnswerClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button &&
                    !string.IsNullOrEmpty(button.ClassId) &&
                    button.CommandParameter is string answerNumberStr &&
                    int.TryParse(answerNumberStr, out int answerNumber))
                {
                    string playerName = button.ClassId;
                    Debug.WriteLine($"OnPlayerGuessAnswerClicked: Player={playerName}, Answer={answerNumber}");

                    // Find the player by name
                    var player = _viewModel.Players.FirstOrDefault(p => p.Name == playerName);
                    if (player != null)
                    {
                        // Get the answer text for this number (answerNumber is 1-4, array index is 0-3)
                        int answerIndex = answerNumber - 1;
                        if (answerIndex >= 0 && answerIndex < _viewModel.CurrentSign.Choices.Count)
                        {
                            string answerText = _viewModel.CurrentSign.Choices[answerIndex];
                            bool isCorrect = answerText == _viewModel.CurrentSign.CorrectAnswer;

                            Debug.WriteLine($"Player {player.Name}: Selected '{answerText}' - {(isCorrect ? "Correct" : "Incorrect")}");

                            // Record the answer with the number and text
                            player.RecordGuessAnswer(answerNumber, answerText, isCorrect);

                            // Show feedback
                            _viewModel.FeedbackText = $"{player.Name} selected {answerNumber}: {answerText}";
                            _viewModel.FeedbackBackgroundColor = Color.FromArgb("#007BFF"); // Use a neutral color
                            _viewModel.IsFeedbackVisible = true;

                            // Force UI refresh
                            _viewModel.OnPropertyChanged(nameof(_viewModel.Players));
                            _viewModel.OnPropertyChanged(nameof(_viewModel.HasAllPlayersAnswered));

                            Debug.WriteLine($"Player {player.Name} updated: HasAnswered={player.HasAnswered}, Score={player.Score}");
                            Debug.WriteLine($"HasAllPlayersAnswered: {_viewModel.HasAllPlayersAnswered}");

                            // Auto-hide feedback after 2 seconds
                            Device.StartTimer(TimeSpan.FromSeconds(2), () => {
                                _viewModel.IsFeedbackVisible = false;
                                return false; // Don't repeat
                            });
                        }
                        else
                        {
                            Debug.WriteLine($"Invalid answer index: {answerIndex}");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Player not found with name: {playerName}");
                    }
                }
                else
                {
                    Debug.WriteLine("Invalid button sender or parameters");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnPlayerGuessAnswerClicked: {ex.Message}");
            }
        }
    }
}