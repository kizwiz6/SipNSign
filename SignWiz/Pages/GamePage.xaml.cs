using com.kizwiz.signwiz.Converters;
using com.kizwiz.signwiz.Enums;
using com.kizwiz.signwiz.Messages;
using com.kizwiz.signwiz.Models;
using com.kizwiz.signwiz.Services;
using com.kizwiz.signwiz.ViewModels;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;

namespace com.kizwiz.signwiz.Pages
{
    /// <summary>
    /// Handles game interactions and video playback for both Guess and Perform modes
    /// </summary>
    public partial class GamePage : ContentPage
    {
        #region Fields
        private readonly GameViewModel _viewModel;
        private readonly IVideoService _videoService;
        private readonly SemaphoreSlim _videoLoadLock = new(1, 1);
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
                _viewModel = new GameViewModel(serviceProvider, videoService, logger, progressService)
                {
                    AnswerCommand = new Command<string>(HandleAnswer),
                    RevealSignCommand = new Command(RevealSign),
                    CurrentVideoSource = MediaSource.FromFile("again.mp4")
                };
                _viewModel.SignRevealRequested += OnSignRevealRequested;

                // Set page reference for animations
                _viewModel.SetPageReference(this);

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

                // Get the video path (file path on all platforms)
                var videoPath = await _videoService.GetVideoPath(videoFileName);
                var source = MediaSource.FromUri(new Uri(videoPath));
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
                await DisplayAlertAsync("Waiting for Players", $"{names} {plural} still to answer.", "OK");
                return;
            }

            // IMMEDIATELY reset all multiplayer button visuals BEFORE executing the command
            if (vm.IsMultiplayer && vm.IsGuessMode)
            {
                Debug.WriteLine("=== RESETTING BUTTONS BEFORE CONFIRM COMMAND ===");
                await ImmediatelyResetAllMultiplayerButtons();
            }

            // All answered -> execute the Confirm command via ViewModel
            if (vm.ConfirmGuessAnswersCommand?.CanExecute(null) ?? false)
            {
                vm.ConfirmGuessAnswersCommand.Execute(null);
            }

            // Scroll back to top of player list after confirming
            if (vm.IsMultiplayer && vm.IsGuessMode)
            {
                await ScrollPlayerListToTop();
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
                    var uri = uriSource.Uri?.ToString();
                    if (uri != null && uri.StartsWith("android.resource://"))
                    {
                        Debug.WriteLine("Android resource - checking resource ID");
                    }
                    else if (uri != null && uri.StartsWith("file://"))
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

        private void OnPageLoaded(object? sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine($"=== OnPageLoaded ===");
                Debug.WriteLine($"IsGuessMode: {ViewModel.IsGuessMode}");
                Debug.WriteLine($"IsMultiplayer: {ViewModel.IsMultiplayer}");
                Debug.WriteLine($"CurrentSign: {ViewModel.CurrentSign?.CorrectAnswer ?? "null"}");

                // Initialize answer button colors from theme
                InitializeAnswerButtonColors();

                // Only initialize MediaElements for current mode
                if (ViewModel.IsGuessMode)
                {
                    if (ViewModel.IsMultiplayer)
                    {
                        _multiplayerGuessVideoElement = FindMediaElement("MultiplayerGuessVideo");
                        Debug.WriteLine($"MultiplayerGuessVideo element found: {_multiplayerGuessVideoElement != null}");
                        if (_multiplayerGuessVideoElement != null)
                        {
                            Debug.WriteLine($"  - IsVisible: {_multiplayerGuessVideoElement.IsVisible}");
                            Debug.WriteLine($"  - Width: {_multiplayerGuessVideoElement.Width}, Height: {_multiplayerGuessVideoElement.Height}");
                        }
                    }
                    else
                    {
                        _sharedVideoElement = FindMediaElement("SharedVideo");
                        Debug.WriteLine($"SharedVideo element found: {_sharedVideoElement != null}");
                        if (_sharedVideoElement != null)
                        {
                            Debug.WriteLine($"  - IsVisible: {_sharedVideoElement.IsVisible}");
                            Debug.WriteLine($"  - Width: {_sharedVideoElement.Width}, Height: {_sharedVideoElement.Height}");
                        }
                    }
                }
                else if (ViewModel.IsPerformMode)
                {
                    if (ViewModel.IsMultiplayer)
                    {
                        _multiplayerPerformVideoElement = FindMediaElement("MultiplayerPerformVideo");
                        Debug.WriteLine($"MultiplayerPerformVideo element found: {_multiplayerPerformVideoElement != null}");
                    }
                    else
                    {
                        _performVideoElement = FindMediaElement("PerformVideo");
                        Debug.WriteLine($"PerformVideo element found: {_performVideoElement != null}");
                    }
                }

                // Set up event handlers
                if (_sharedVideoElement != null)
                {
                    _sharedVideoElement.PropertyChanged += OnSharedVideoPropertyChanged;
                }

                // Trigger video load if we have a current sign
                if (ViewModel.CurrentSign != null)
                {
                    Debug.WriteLine("CurrentSign exists, triggering video load...");
                    _ = LoadVideoForCurrentSign();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnPageLoaded: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
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

            // Register for app lifecycle messages
            WeakReferenceMessenger.Default.Register<AppSleepMessage>(this, (r, m) =>
            {
                MainThread.BeginInvokeOnMainThread(OnAppSleep);
            });
            WeakReferenceMessenger.Default.Register<AppResumeMessage>(this, (r, m) =>
            {
                MainThread.BeginInvokeOnMainThread(OnAppResume);
            });

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

                // Unregister lifecycle messages
                WeakReferenceMessenger.Default.Unregister<AppSleepMessage>(this);
                WeakReferenceMessenger.Default.Unregister<AppResumeMessage>(this);

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

        #region App Lifecycle Pause/Resume
        /// <summary>
        /// Called when the app is backgrounded. Pauses video playback and game timer.
        /// </summary>
        private void OnAppSleep()
        {
            if (_isDisposed || _viewModel == null) return;

            Debug.WriteLine("GamePage: App going to sleep — pausing game");

            _viewModel.PauseGame();

            // Pause all active MediaElements
            try
            {
                _sharedVideoElement?.Pause();
                _multiplayerGuessVideoElement?.Pause();
                _performVideoElement?.Pause();
                _multiplayerPerformVideoElement?.Pause();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error pausing media elements: {ex.Message}");
            }
        }

        /// <summary>
        /// Called when the app returns to the foreground. Does NOT auto-resume;
        /// the user must press the Resume button shown by the pause overlay.
        /// </summary>
        private void OnAppResume()
        {
            if (_isDisposed || _viewModel == null) return;

            Debug.WriteLine("GamePage: App resumed — waiting for user to press Resume");
            // Intentionally left empty: the user taps the Resume button in the overlay,
            // which calls ResumeCommand on the ViewModel, which resumes the timer.
            // Video playback is resumed below when IsGamePaused changes to false.
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

                        // Reset button colors when new sign loads
                        InitializeAnswerButtonColors();

                        // Reset multiplayer answer buttons to default state
                        if (_viewModel.IsMultiplayer && _viewModel.IsGuessMode)
                        {
                            Debug.WriteLine("=== TRIGGERING MULTIPLAYER BUTTON RESET ===");

                            // Force CollectionView to refresh by re-raising the Players collection
                            _viewModel.OnPropertyChanged(nameof(_viewModel.Players));

                            // Small delay to let the CollectionView update
                            await Task.Delay(100);

                            // Now reset the buttons
                            ResetMultiplayerAnswerButtons();
                        }

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

                    if (e.PropertyName == nameof(GameViewModel.IsGamePaused) && !_viewModel.IsGamePaused)
                    {
                        // Game was just resumed via the Resume button — restart video playback
                        Debug.WriteLine("GamePage: IsGamePaused changed to false — resuming video");
                        try
                        {
                            _sharedVideoElement?.Play();
                            _multiplayerGuessVideoElement?.Play();
                            _performVideoElement?.Play();
                            _multiplayerPerformVideoElement?.Play();
                        }
                        catch (Exception ex2)
                        {
                            Debug.WriteLine($"Error resuming media elements: {ex2.Message}");
                        }
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
            var result = converter.Convert(player, typeof(PlayerAnswerParameter), true, System.Globalization.CultureInfo.InvariantCulture);

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
        /// Handles when an answer button is loaded - sets initial theme color immediately
        /// </summary>
        private void OnAnswerButtonLoaded(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                Color themeColor = GetThemeColor();
                var themeBrush = new SolidColorBrush(themeColor);
                button.Background = themeBrush;
                Debug.WriteLine($"Answer button '{button.Text}' loaded with theme color");
            }
        }

        /// <summary>
        /// Handles when a single player perform button is loaded - sets color immediately
        /// </summary>
        private void OnPerformButtonLoaded(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                // Determine color based on button name
                Color buttonColor;
                if (button.Text != null && button.Text.Contains("RIGHT"))
                {
                    buttonColor = Color.FromArgb("#228B22"); // ForestGreen
                }
                else
                {
                    buttonColor = Color.FromArgb("#DC143C"); // Crimson
                }

                var colorBrush = new SolidColorBrush(buttonColor);
                button.Background = colorBrush;
                Debug.WriteLine($"Perform button '{button.Text}' loaded with color {buttonColor}");
            }
        }

        /// <summary>
        /// Handles when a multiplayer player perform button is loaded - sets color immediately
        /// </summary>
        private void OnPlayerPerformButtonLoaded(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                // Determine color based on button text (✓ or ✗)
                Color buttonColor;
                if (button.Text == "✓")
                {
                    buttonColor = Color.FromArgb("#228B22"); // ForestGreen
                }
                else
                {
                    buttonColor = Color.FromArgb("#DC143C"); // Crimson
                }

                var colorBrush = new SolidColorBrush(buttonColor);
                button.Background = colorBrush;
                Debug.WriteLine($"Player perform button '{button.Text}' for player {button.ClassId} loaded with color {buttonColor}");
            }
        }

        /// <summary>
        /// Handles when a multiplayer player answer button is loaded - sets initial theme color immediately
        /// </summary>
        private void OnPlayerAnswerButtonLoaded(object sender, EventArgs e)
        {
            if (sender is Button button && !string.IsNullOrEmpty(button.ClassId))
            {
                string playerName = button.ClassId;
                var player = _viewModel?.Players?.FirstOrDefault(p => p.Name == playerName);

                // Parse the answer number from CommandParameter
                int buttonAnswerNumber = 0;
                if (button.CommandParameter is string paramStr && int.TryParse(paramStr, out int parsed))
                {
                    buttonAnswerNumber = parsed;
                }

                Debug.WriteLine($"=== OnPlayerAnswerButtonLoaded: Player={playerName}, ButtonAnswer={buttonAnswerNumber}, PlayerSelectedAnswer={player?.SelectedAnswer ?? -999} ===");

                // Check if this button should be highlighted based on player's current selection
                bool shouldHighlight = player != null && 
                                      player.SelectedAnswer == buttonAnswerNumber && 
                                      buttonAnswerNumber > 0;

                if (shouldHighlight)
                {
                    // Apply selection highlight
                    Color selectedColor = Color.FromArgb("#4169E1"); // Royal Blue
                    button.Background = new SolidColorBrush(selectedColor);
                    button.Scale = 1.1;
                    Debug.WriteLine($">>> HIGHLIGHTED button {buttonAnswerNumber} for {playerName}");
                }
                else
                {
                    // Apply default theme color
                    Color themeColor = GetThemeColor();
                    button.Background = new SolidColorBrush(themeColor);
                    button.Scale = 1.0;
                    Debug.WriteLine($">>> DEFAULT theme for button {buttonAnswerNumber} for {playerName}");
                }
            }
        }

        /// <summary>
        /// Initializes answer button colors from the current theme
        /// </summary>
        private void InitializeAnswerButtonColors()
        {
            try
            {
                Color themeColor = GetThemeColor();
                Debug.WriteLine($"InitializeAnswerButtonColors: Setting buttons to theme color {themeColor}");

                var button1 = this.FindByName<Button>("AnswerButton1");
                var button2 = this.FindByName<Button>("AnswerButton2");
                var button3 = this.FindByName<Button>("AnswerButton3");
                var button4 = this.FindByName<Button>("AnswerButton4");

                // Use Background property instead of BackgroundColor to override the implicit style
                var themeBrush = new SolidColorBrush(themeColor);

                if (button1 != null) button1.Background = themeBrush;
                if (button2 != null) button2.Background = themeBrush;
                if (button3 != null) button3.Background = themeBrush;
                if (button4 != null) button4.Background = themeBrush;

                Debug.WriteLine($"Answer buttons initialized: B1={button1 != null}, B2={button2 != null}, B3={button3 != null}, B4={button4 != null}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in InitializeAnswerButtonColors: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the current theme's Primary color
        /// </summary>
        private Color GetThemeColor()
        {
            try
            {
                if (Application.Current?.Resources != null &&
                    Application.Current.Resources.TryGetValue("Primary", out var colorValue) &&
                    colorValue is Color themeColor)
                {
                    return themeColor;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting theme color: {ex.Message}");
            }

            // Fallback to a default blue color
            return Color.FromArgb("#007BFF");
        }

        /// <summary>
        /// Finds all buttons in CollectionView items by directly iterating the visual children
        /// </summary>
        private List<Button> FindButtonsInCollectionView(CollectionView collectionView)
        {
            var buttons = new List<Button>();

            if (collectionView == null)
                return buttons;

            try
            {
                // Try to find buttons by recursively searching all visual descendants
                var queue = new Queue<Element>();
                queue.Enqueue(collectionView);

                while (queue.Count > 0)
                {
                    var element = queue.Dequeue();

                    if (element is Button button && 
                        !string.IsNullOrEmpty(button.ClassId) &&
                        button.CommandParameter is string paramStr &&
                        int.TryParse(paramStr, out _))
                    {
                        buttons.Add(button);
                        Debug.WriteLine($"  Found button: ClassId={button.ClassId}, CommandParameter={button.CommandParameter}");
                    }

                    // Add all visual children to the queue
                    if (element is Layout layout)
                    {
                        foreach (var child in layout.Children)
                        {
                            if (child is Element childElement)
                                queue.Enqueue(childElement);
                        }
                    }
                    else if (element is ContentView contentView && contentView.Content is Element content)
                    {
                        queue.Enqueue(content);
                    }
                    else if (element is ScrollView scrollView && scrollView.Content is Element scrollContent)
                    {
                        queue.Enqueue(scrollContent);
                    }
                    else if (element is Border border && border.Content is Element borderContent)
                    {
                        queue.Enqueue(borderContent);
                    }

                    // Handle CollectionView - note: we rely on the layout traversal above
                    // The buttons are in the ItemTemplate and will be found through the layout hierarchy
                    if (element is CollectionView cv)
                    {
                        Debug.WriteLine($"  CollectionView has {cv.ItemsSource?.Cast<object>().Count() ?? 0} items");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in FindButtonsInCollectionView: {ex.Message}");
            }

            return buttons;
        }

        /// <summary>
        /// Resets all multiplayer answer buttons to default state
        /// </summary>
        private async void ResetMultiplayerAnswerButtons()
        {
            try
            {
                Debug.WriteLine("=== ResetMultiplayerAnswerButtons called ===");

                // Give the CollectionView time to update its items
                await Task.Delay(100);

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    Color defaultColor = GetThemeColor();
                    var defaultBrush = new SolidColorBrush(defaultColor);

                    // Find the specific CollectionView by name
                    var collectionView = this.FindByName<CollectionView>("MultiplayerPlayersCollectionView");
                    if (collectionView == null)
                    {
                        Debug.WriteLine("ERROR: MultiplayerPlayersCollectionView not found!");
                        return;
                    }

                    Debug.WriteLine($"Found CollectionView with {collectionView.ItemsSource?.Cast<object>().Count() ?? 0} items");

                    // Use our custom method to find buttons
                    var playerButtons = FindButtonsInCollectionView(collectionView);

                    Debug.WriteLine($"Found {playerButtons.Count} player answer buttons to reset");

                    foreach (var button in playerButtons)
                    {
                        button.Background = defaultBrush;
                        button.Scale = 1.0;
                        Debug.WriteLine($"  Reset button for player {button.ClassId}, answer {button.CommandParameter}");
                    }

                    // If no buttons were found, try again after a longer delay
                    if (playerButtons.Count == 0)
                    {
                        Debug.WriteLine("No buttons found in CollectionView, retrying after longer delay...");
                        await Task.Delay(300);

                        playerButtons = FindButtonsInCollectionView(collectionView);

                        Debug.WriteLine($"Retry found {playerButtons.Count} player answer buttons");

                        foreach (var button in playerButtons)
                        {
                            button.Background = defaultBrush;
                            button.Scale = 1.0;
                            Debug.WriteLine($"  Reset button for player {button.ClassId}, answer {button.CommandParameter}");
                        }
                    }

                    Debug.WriteLine($"Reset complete: {playerButtons.Count} buttons processed");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ResetMultiplayerAnswerButtons: {ex.Message}");
            }
        }

        /// <summary>
        /// Immediately resets all multiplayer answer buttons - called right before Confirm
        /// </summary>
        private async Task ImmediatelyResetAllMultiplayerButtons()
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Color defaultColor = GetThemeColor();
                    var defaultBrush = new SolidColorBrush(defaultColor);

                    // Find the specific CollectionView by name
                    var collectionView = this.FindByName<CollectionView>("MultiplayerPlayersCollectionView");
                    if (collectionView == null)
                    {
                        Debug.WriteLine("ERROR: MultiplayerPlayersCollectionView not found in ImmediatelyResetAllMultiplayerButtons!");
                        return;
                    }

                    // Use our custom method to find buttons
                    var allButtons = FindButtonsInCollectionView(collectionView);

                    Debug.WriteLine($"=== RESETTING BUTTONS BEFORE CONFIRM COMMAND ===");
                    Debug.WriteLine($"ImmediatelyResetAllMultiplayerButtons: Resetting {allButtons.Count} buttons in CollectionView");

                    foreach (var button in allButtons)
                    {
                        button.Background = defaultBrush;
                        button.Scale = 1.0;
                        Debug.WriteLine($"  Reset button {button.CommandParameter} for {button.ClassId}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ImmediatelyResetAllMultiplayerButtons: {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively finds all elements of a specific type in the visual tree
        /// </summary>
        private List<T> FindAllElements<T>(Element element) where T : Element
        {
            var elements = new List<T>();

            if (element is T item)
            {
                elements.Add(item);
            }

            if (element is Layout layout)
            {
                foreach (var child in layout.Children)
                {
                    if (child is Element childElement)
                    {
                        elements.AddRange(FindAllElements<T>(childElement));
                    }
                }
            }
            else if (element is ContentView contentView && contentView.Content is Element content)
            {
                elements.AddRange(FindAllElements<T>(content));
            }
            else if (element is ScrollView scrollView && scrollView.Content is Element scrollContent)
            {
                elements.AddRange(FindAllElements<T>(scrollContent));
            }
            else if (element is Border border && border.Content is Element borderContent)
            {
                elements.AddRange(FindAllElements<T>(borderContent));
            }
            else if (element is CollectionView collectionView)
            {
                // CollectionView items are handled dynamically, skip for now
            }

            return elements;
        }

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

                        // Use the new RecordAnswer method (allows changing answers and handles scoring)
                        player.RecordAnswer(true);

                        // Show feedback - use perform feedback if in perform mode
                        if (_viewModel.IsPerformMode && _viewModel.IsMultiplayer)
                        {
                            ShowMultiplayerPerformFeedback($"{player.Name} got it right! ✓", true);
                        }
                        else
                        {
                            _viewModel.FeedbackText = $"{player.Name} got it right!";
                            _viewModel.FeedbackBackgroundColor = _viewModel.GetFeedbackColor(true);
                            _viewModel.IsFeedbackVisible = true;

                            // Auto-hide feedback after 2 seconds
                            Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(2), () => {
                                _viewModel.IsFeedbackVisible = false;
                            });
                        }

                        // Force UI refresh
                        _viewModel.OnPropertyChanged(nameof(_viewModel.Players));
                        _viewModel.OnPropertyChanged(nameof(_viewModel.HasAllPlayersAnswered));

                        Debug.WriteLine($"Player {player.Name} updated: HasAnswered={player.HasAnswered}, GotCurrentAnswerCorrect={player.GotCurrentAnswerCorrect}, Score={player.Score}");
                        Debug.WriteLine($"HasAllPlayersAnswered: {_viewModel.HasAllPlayersAnswered}");
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

                        // Use the new RecordAnswer method (allows changing answers and handles scoring)
                        player.RecordAnswer(false);

                        // Show feedback - use perform feedback if in perform mode
                        if (_viewModel.IsPerformMode && _viewModel.IsMultiplayer)
                        {
                            ShowMultiplayerPerformFeedback($"{player.Name} got it wrong ✗", false);
                        }
                        else
                        {
                            _viewModel.FeedbackText = $"{player.Name} got it wrong!";
                            _viewModel.FeedbackBackgroundColor = _viewModel.GetFeedbackColor(false);
                            _viewModel.IsFeedbackVisible = true;

                            // Auto-hide feedback after 2 seconds
                            Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(2), () => {
                                _viewModel.IsFeedbackVisible = false;
                            });
                        }

                        // Force UI refresh
                        _viewModel.OnPropertyChanged(nameof(_viewModel.Players));
                        _viewModel.OnPropertyChanged(nameof(_viewModel.HasAllPlayersAnswered));

                        Debug.WriteLine($"Player {player.Name} updated: HasAnswered={player.HasAnswered}, GotCurrentAnswerCorrect={player.GotCurrentAnswerCorrect}, Score={player.Score}");
                        Debug.WriteLine($"HasAllPlayersAnswered: {_viewModel.HasAllPlayersAnswered}");
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

                await DisplayAlertAsync(
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
            await ViewModel.ShowFeedbackAndContinue(allPlayersCorrect);
        }

        /// <summary>
        /// Handles when a player selects an answer (1-4) in multiplayer Guess mode
        /// </summary>
        private void OnPlayerGuessAnswerClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button clickedButton &&
                    !string.IsNullOrEmpty(clickedButton.ClassId) &&
                    clickedButton.CommandParameter is string answerNumberStr &&
                    int.TryParse(answerNumberStr, out int answerNumber))
                {
                    string playerName = clickedButton.ClassId;
                    Debug.WriteLine($"OnPlayerGuessAnswerClicked: Player={playerName}, Answer={answerNumber}");

                    // Find the player by name
                    var player = _viewModel.Players.FirstOrDefault(p => p.Name == playerName);
                    if (player != null && _viewModel.CurrentSign?.Choices != null)
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

                            // Highlight the selected button and reset others for this player
                            HighlightPlayerAnswerButton(clickedButton, playerName, answerNumber);

                            // Show feedback badge at bottom of video
                            ShowMultiplayerFeedback($"{player.Name} selected:\n{answerNumber}. {answerText}");

                            // Force UI refresh
                            _viewModel.OnPropertyChanged(nameof(_viewModel.Players));
                            _viewModel.OnPropertyChanged(nameof(_viewModel.HasAllPlayersAnswered));

                            Debug.WriteLine($"Player {player.Name} updated: HasAnswered={player.HasAnswered}, Score={player.Score}");
                            Debug.WriteLine($"HasAllPlayersAnswered: {_viewModel.HasAllPlayersAnswered}");
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

        /// <summary>
        /// Shows multiplayer feedback badge at bottom of video
        /// </summary>
        private async void ShowMultiplayerFeedback(string message)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var feedbackBadge = this.FindByName<Border>("MultiplayerFeedbackBadge");
                    var feedbackText = this.FindByName<Label>("MultiplayerFeedbackText");

                    if (feedbackBadge != null && feedbackText != null)
                    {
                        feedbackText.Text = message;
                        feedbackBadge.Opacity = 0;
                        feedbackBadge.IsVisible = true;

                        // Fade in
                        await feedbackBadge.FadeTo(1, 250);

                        // Keep visible for 2 seconds
                        await Task.Delay(2000);

                        // Fade out
                        await feedbackBadge.FadeTo(0, 250);
                        feedbackBadge.IsVisible = false;
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ShowMultiplayerFeedback: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows multiplayer perform feedback badge at bottom of video
        /// </summary>
        private async void ShowMultiplayerPerformFeedback(string message, bool isCorrect)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var feedbackBadge = this.FindByName<Border>("MultiplayerPerformFeedbackBadge");
                    var feedbackText = this.FindByName<Label>("MultiplayerPerformFeedbackText");

                    if (feedbackBadge != null && feedbackText != null)
                    {
                        feedbackText.Text = message;
                        // Set background color based on correct/incorrect
                        feedbackBadge.BackgroundColor = isCorrect ? Color.FromArgb("#28a745") : Color.FromArgb("#dc3545");
                        feedbackBadge.Opacity = 0;
                        feedbackBadge.IsVisible = true;

                        // Fade in
                        await feedbackBadge.FadeToAsync(1, 250);

                        // Keep visible for 2 seconds
                        await Task.Delay(2000);

                        // Fade out
                        await feedbackBadge.FadeToAsync(0, 250);
                        feedbackBadge.IsVisible = false;
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ShowMultiplayerPerformFeedback: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows single player perform feedback badge at bottom of video
        /// </summary>
        public async Task ShowSinglePlayerPerformFeedback(string message, bool isCorrect)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var feedbackBadge = this.FindByName<Border>("SinglePlayerPerformFeedbackBadge");
                    var feedbackText = this.FindByName<Label>("SinglePlayerPerformFeedbackText");

                    if (feedbackBadge != null && feedbackText != null)
                    {
                        feedbackText.Text = message;
                        // Set background color based on correct/incorrect
                        feedbackBadge.BackgroundColor = isCorrect ? Color.FromArgb("#28a745") : Color.FromArgb("#dc3545");
                        feedbackBadge.Opacity = 0;
                        feedbackBadge.IsVisible = true;

                        // Fade in
                        await feedbackBadge.FadeToAsync(1, 250);

                        // Keep visible for 1.5 seconds
                        await Task.Delay(1500);

                        // Fade out
                        await feedbackBadge.FadeToAsync(0, 250);
                        feedbackBadge.IsVisible = false;
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ShowSinglePlayerPerformFeedback: {ex.Message}");
            }
        }

        /// <summary>
        /// Highlights the selected player answer button and resets others
        /// </summary>
        private void HighlightPlayerAnswerButton(Button selectedButton, string playerName, int selectedAnswer)
        {
            try
            {
                // Find all buttons for this player in the CollectionView
                var collectionView = this.FindByName<CollectionView>("ItemsSource");

                // Get the parent StackLayout containing all 4 buttons
                var parent = selectedButton.Parent as StackLayout;
                if (parent != null)
                {
                    // Get theme colors
                    Color selectedColor = Color.FromArgb("#4169E1"); // Royal Blue - selected but not revealed
                    Color defaultColor = GetThemeColor();

                    var selectedBrush = new SolidColorBrush(selectedColor);
                    var defaultBrush = new SolidColorBrush(defaultColor);

                    // Reset all buttons to default, highlight the selected one
                    foreach (var child in parent.Children)
                    {
                        if (child is Button btn)
                        {
                            if (btn == selectedButton)
                            {
                                btn.Background = selectedBrush;
                                btn.Scale = 1.1;
                                Debug.WriteLine($"Highlighted button {btn.Text} for {playerName}");
                            }
                            else
                            {
                                btn.Background = defaultBrush;
                                btn.Scale = 1.0;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in HighlightPlayerAnswerButton: {ex.Message}");
            }
        }

        /// <summary>
        /// Scrolls the multiplayer player list back to the top
        /// </summary>
        private async Task ScrollPlayerListToTop()
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var collectionView = this.FindByName<CollectionView>("MultiplayerPlayersCollectionView");
                    if (collectionView != null && _viewModel?.Players != null && _viewModel.Players.Count > 0)
                    {
                        // Scroll to the first player in the list
                        collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: true);
                        Debug.WriteLine("Scrolled player list to top");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error scrolling player list to top: {ex.Message}");
            }
        }

        #region Visual Feedback Animations
        /// <summary>
        /// Shows button flash feedback, border glow, and correct answer badge for Guess mode
        /// </summary>
        /// <param name="selectedAnswer">The answer the user selected</param>
        /// <param name="isCorrect">True if the answer was correct</param>
        public async Task ShowButtonFeedback(string selectedAnswer, bool isCorrect)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    // Find the video border for glow effect
                    Border? videoBorder = null;
                    if (_viewModel.IsGuessMode)
                    {
                        videoBorder = this.FindByName<Border>("VideoGuessBorder");
                    }
                    else if (_viewModel.IsPerformMode)
                    {
                        if (_viewModel.IsMultiplayer)
                        {
                            videoBorder = this.FindByName<Border>("VideoPerformMultiplayerBorder");
                        }
                        else
                        {
                            videoBorder = this.FindByName<Border>("VideoPerformBorder");
                        }
                    }

                    // Find the badge and label
                    var correctAnswerBadge = this.FindByName<Border>("CorrectAnswerBadge");
                    var correctAnswerText = this.FindByName<Label>("CorrectAnswerText");

                    // Find answer buttons by name (only in single-player Guess mode)
                    Button? selectedButton = null;
                    Button? correctButton = null;

                    Debug.WriteLine($"=== ShowButtonFeedback ===");
                    Debug.WriteLine($"Selected Answer: {selectedAnswer}");
                    Debug.WriteLine($"Correct Answer: {_viewModel.CurrentSign?.CorrectAnswer}");
                    Debug.WriteLine($"Is Correct: {isCorrect}");

                    // Try to find buttons by name first (more reliable)
                    if (_viewModel.IsGuessMode && !_viewModel.IsMultiplayer)
                    {
                        var button1 = this.FindByName<Button>("AnswerButton1");
                        var button2 = this.FindByName<Button>("AnswerButton2");
                        var button3 = this.FindByName<Button>("AnswerButton3");
                        var button4 = this.FindByName<Button>("AnswerButton4");

                        Debug.WriteLine($"Named buttons found: Button1={button1 != null}, Button2={button2 != null}, Button3={button3 != null}, Button4={button4 != null}");

                        // Check each button to find selected and correct
                        foreach (var button in new[] { button1, button2, button3, button4 })
                        {
                            if (button == null) continue;

                            Debug.WriteLine($"  Checking button: Text='{button.Text}', BackgroundColor={button.BackgroundColor}");

                            if (button.Text == selectedAnswer)
                            {
                                selectedButton = button;
                                Debug.WriteLine($"  ✓ Found SELECTED button: '{button.Text}'");
                            }
                            if (button.Text == _viewModel.CurrentSign?.CorrectAnswer)
                            {
                                correctButton = button;
                                Debug.WriteLine($"  ✓ Found CORRECT button: '{button.Text}'");
                            }
                        }
                    }

                    Debug.WriteLine($"Selected button found: {selectedButton != null}");
                    Debug.WriteLine($"Correct button found: {correctButton != null}");

                    if (isCorrect)
                    {
                        // Correct answer: Flash button green and glow border green
                        var tasks = new List<Task>();

                        if (selectedButton != null)
                        {
                            Debug.WriteLine($"Starting green flash for button: '{selectedButton.Text}'");
                            tasks.Add(FlashButtonGreen(selectedButton));
                        }
                        else
                        {
                            Debug.WriteLine("WARNING: Selected button not found for green flash!");
                        }

                        if (videoBorder != null)
                        {
                            tasks.Add(AnimateGreenGlow(videoBorder));
                        }

                        await Task.WhenAll(tasks);
                    }
                    else
                    {
                        // Incorrect answer: Flash selected button red, correct button green, and glow border red
                        var tasks = new List<Task>();

                        if (selectedButton != null)
                        {
                            Debug.WriteLine($"Starting red flash for button: '{selectedButton.Text}'");
                            tasks.Add(FlashButtonRed(selectedButton));
                        }
                        else
                        {
                            Debug.WriteLine("WARNING: Selected button not found for red flash!");
                        }

                        if (correctButton != null)
                        {
                            Debug.WriteLine($"Starting green flash for correct button: '{correctButton.Text}'");
                            tasks.Add(FlashButtonGreen(correctButton));
                        }
                        else
                        {
                            Debug.WriteLine("WARNING: Correct button not found for green flash!");
                        }

                        if (videoBorder != null)
                        {
                            tasks.Add(AnimateRedGlow(videoBorder));
                        }

                        await Task.WhenAll(tasks);

                        // Show the correct answer badge (just the sign name, no checkmark)
                        if (correctAnswerBadge != null && correctAnswerText != null)
                        {
                            correctAnswerText.Text = _viewModel.CurrentSign?.CorrectAnswer ?? "";
                            correctAnswerBadge.Opacity = 0;
                            correctAnswerBadge.IsVisible = true;
                            await correctAnswerBadge.FadeToAsync(1, 300);
                            await Task.Delay(1200); // Keep visible for 1.2 seconds
                            await correctAnswerBadge.FadeToAsync(0, 300);
                            correctAnswerBadge.IsVisible = false;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ShowButtonFeedback: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Flash a button green - keeps the color until next sign loads
        /// </summary>
        private async Task FlashButtonGreen(Button button)
        {
            try
            {
                // Create a solid green color
                var greenColor = Color.FromArgb("#28a745");
                var greenBrush = new SolidColorBrush(greenColor);

                Debug.WriteLine($"FlashButtonGreen START: Button '{button.Text}', Current BG: {button.Background}");

                // Change to green - use Background property to override implicit style
                button.Background = greenBrush;
                button.TextColor = Colors.White;

                Debug.WriteLine($"FlashButtonGreen SET: Button '{button.Text}' changed to GREEN - will stay until next sign");

                // No restoration - color persists until InitializeAnswerButtonColors is called for next sign
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in FlashButtonGreen: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Flash a button red - keeps the color until next sign loads
        /// </summary>
        private async Task FlashButtonRed(Button button)
        {
            try
            {
                // Create a solid red color
                var redColor = Color.FromArgb("#dc3545");
                var redBrush = new SolidColorBrush(redColor);

                Debug.WriteLine($"FlashButtonRed START: Button '{button.Text}', Current BG: {button.Background}");

                // Change to red - use Background property to override implicit style
                button.Background = redBrush;
                button.TextColor = Colors.White;

                Debug.WriteLine($"FlashButtonRed SET: Button '{button.Text}' changed to RED - will stay until next sign");

                // No restoration - color persists until InitializeAnswerButtonColors is called for next sign
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in FlashButtonRed: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Animates green glow effect on video border for correct answers
        /// </summary>
#pragma warning disable CA1416 // Platform compatibility - MAUI cross-platform code
        private async Task AnimateGreenGlow(Border videoBorder)
        {
            try
            {
                var greenColor = Color.FromArgb("#28a745");

                // Pulse: Grow glow
                videoBorder.Stroke = greenColor;
                var animation = new Animation(v => videoBorder.StrokeThickness = v, 0, 8);
                animation.Commit(videoBorder, "GreenGlow", 16, 500, Easing.CubicOut);
                await Task.Delay(500);

                // Hold
                await Task.Delay(500);

                // Fade out
                var animation2 = new Animation(v => videoBorder.StrokeThickness = v, 8, 0);
                animation2.Commit(videoBorder, "GreenFadeOut", 16, 500, Easing.CubicIn);
                await Task.Delay(500);

                // Reset
                videoBorder.Stroke = Colors.Transparent;
                videoBorder.StrokeThickness = 0;
            }
#pragma warning restore CA1416
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in AnimateGreenGlow: {ex.Message}");
            }
        }

        /// <summary>
        /// Animates red glow effect on video border for incorrect answers
        /// </summary>
        private async Task AnimateRedGlow(Border videoBorder)
        {
            try
            {
                var redColor = Color.FromArgb("#dc3545");

                // Flash red
                videoBorder.Stroke = redColor;
                videoBorder.StrokeThickness = 8;
                await Task.Delay(500);

                // Pulse
                var animation = new Animation(v => videoBorder.StrokeThickness = v, 8, 6);
                animation.Commit(videoBorder, "RedPulse", 16, 250, Easing.Linear);
                await Task.Delay(250);

                var animation2 = new Animation(v => videoBorder.StrokeThickness = v, 6, 8);
                animation2.Commit(videoBorder, "RedPulse2", 16, 250, Easing.Linear);
                await Task.Delay(250);

                // Fade out
                var animation3 = new Animation(v => videoBorder.StrokeThickness = v, 8, 0);
                animation3.Commit(videoBorder, "RedFadeOut", 16, 500, Easing.CubicIn);
                await Task.Delay(500);

                // Reset
                videoBorder.Stroke = Colors.Transparent;
                videoBorder.StrokeThickness = 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in AnimateRedGlow: {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively finds all Button elements in the visual tree
        /// </summary>
        private List<Button> FindAllButtons(Element element)
        {
            var buttons = new List<Button>();

            if (element is Button button)
            {
                buttons.Add(button);
            }

            if (element is Layout layout)
            {
                foreach (var child in layout.Children)
                {
                    if (child is Element childElement)
                    {
                        buttons.AddRange(FindAllButtons(childElement));
                    }
                }
            }
            else if (element is ContentView contentView && contentView.Content is Element content)
            {
                buttons.AddRange(FindAllButtons(content));
            }
            else if (element is ScrollView scrollView && scrollView.Content is Element scrollContent)
            {
                buttons.AddRange(FindAllButtons(scrollContent));
            }
            else if (element is Border border && border.Content is Element borderContent)
            {
                buttons.AddRange(FindAllButtons(borderContent));
            }

            return buttons;
        }
        #endregion
    }
}