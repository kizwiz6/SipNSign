using com.kizwiz.signwiz.Models;
using com.kizwiz.signwiz.Services;
using CommunityToolkit.Maui.Views;
using System.Diagnostics;

namespace com.kizwiz.signwiz.Pages;

/// <summary>
/// Displays a sign's video and name for review/learning.
/// </summary>
public partial class SignDetailsPage : ContentPage
{
    private readonly SignModel _sign;
    private readonly IVideoService _videoService;
    private bool _videoSourceSet;

    /// <summary>
    /// Initializes a new instance of SignDetailsPage for the given sign.
    /// </summary>
    /// <param name="sign">The sign to display for review.</param>
    public SignDetailsPage(SignModel sign)
    {
        try
        {
            InitializeComponent();

            _sign = sign;

            var services = Application.Current?.Handler?.MauiContext?.Services;
            _videoService = services?.GetRequiredService<IVideoService>()
                ?? throw new InvalidOperationException("IVideoService not available");

            SignNameLabel.Text = _sign.CorrectAnswer;
            CategoryLabel.Text = _sign.Category.ToString();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in SignDetailsPage constructor: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Delay video load to ensure Android SurfaceView is fully created
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(300), async () =>
        {
            Debug.WriteLine("SignDetailsPage: Dispatched, loading video...");
            await LoadVideoAsync();

#if ANDROID
            // Android-specific: Force the video surface to render properly
            await Task.Delay(200);
            if (SignVideo?.Handler?.PlatformView is Android.Views.View androidView)
            {
                Debug.WriteLine("SignDetailsPage: Applying Android-specific video rendering fixes");
                Debug.WriteLine($"Android view type: {androidView.GetType().Name}");

                // Find the SurfaceView child if it exists
                if (androidView is Android.Views.ViewGroup viewGroup)
                {
                    for (int i = 0; i < viewGroup.ChildCount; i++)
                    {
                        var child = viewGroup.GetChildAt(i);
                        Debug.WriteLine($"  Child {i}: {child?.GetType().Name}");

                        if (child is Android.Views.SurfaceView surfaceView)
                        {
                            Debug.WriteLine("  Found SurfaceView, applying z-order fixes");
                            surfaceView.SetZOrderOnTop(false);
                            surfaceView.SetZOrderMediaOverlay(true);
                            surfaceView.BringToFront();
                            surfaceView.Invalidate();
                        }
                    }
                }

                // Force parent to redraw
                androidView.BringToFront();
                androidView.Invalidate();
                androidView.RequestLayout();

                Debug.WriteLine("SignDetailsPage: Android video rendering fixes applied");
            }
#endif
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        try
        {
            SignVideo.Stop();
            SignVideo.Source = null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error stopping video: {ex.Message}");
        }
    }

    private async Task LoadVideoAsync()
    {
        try
        {
            var videoFileName = Path.GetFileName(_sign.VideoPath);
            Debug.WriteLine($"=== SignDetailsPage LoadVideo: {videoFileName} ===");
            Debug.WriteLine($"SignVideo element exists: {SignVideo != null}");

            if (SignVideo != null)
            {
                Debug.WriteLine($"  - IsVisible: {SignVideo.IsVisible}");
                Debug.WriteLine($"  - Width: {SignVideo.Width}, Height: {SignVideo.Height}");
                Debug.WriteLine($"  - Bounds: {SignVideo.Bounds}");
            }

            // Wait for the MediaElement handler to be ready (same pattern as GamePage)
            int retries = 0;
            while (SignVideo.Handler == null && retries < 10)
            {
                Debug.WriteLine($"Waiting for SignVideo handler... (attempt {retries + 1})");
                await Task.Delay(100);
                retries++;
            }

            if (SignVideo.Handler == null)
            {
                Debug.WriteLine("ERROR: SignVideo handler never initialized!");
                return;
            }

            Debug.WriteLine($"SignVideo handler ready after {retries} retries");

            var videoPath = await _videoService.GetVideoPath(videoFileName);
            Debug.WriteLine($"Video path resolved: {videoPath}");
            Debug.WriteLine($"File exists: {File.Exists(videoPath)}");

            var source = MediaSource.FromUri(new Uri(videoPath));

            // Already on main thread via Dispatcher, set source directly
            try
            {
                // Set flag BEFORE assigning source, because OnMediaOpened
                // can fire synchronously during the Source setter
                _videoSourceSet = true;

                SignVideo.Stop();
                SignVideo.Source = null;
                SignVideo.Source = source;
                SignVideo.IsVisible = true;
                Debug.WriteLine($"Video source set, State: {SignVideo.CurrentState}");
                Debug.WriteLine($"After setting source - Width: {SignVideo.Width}, Height: {SignVideo.Height}");
                // ShouldAutoPlay=True handles playback start; OnMediaOpened is the safety net
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting video source: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading video: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            await DisplayAlertAsync("Error", "Unable to load the sign video.", "OK");
        }
    }

    private void OnMediaOpened(object? sender, EventArgs e)
    {
        Debug.WriteLine($"SignDetailsPage: Media opened, source set: {_videoSourceSet}");
        if (!_videoSourceSet) return;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                Debug.WriteLine($"SignDetailsPage: Starting play sequence...");

                // Android workaround: Force a layout refresh to ensure SurfaceView renders
                SignVideo.IsVisible = false;
                await Task.Delay(100);
                SignVideo.IsVisible = true;

                // Force layout invalidation
                SignVideo.InvalidateMeasure();
                await Task.Delay(100);

                SignVideo.Play();
                Debug.WriteLine($"SignDetailsPage: Play command sent, State: {SignVideo.CurrentState}");

                // Double-check after a moment
                await Task.Delay(500);
                Debug.WriteLine($"SignDetailsPage: After delay - State: {SignVideo.CurrentState}, IsVisible: {SignVideo.IsVisible}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnMediaOpened: {ex.Message}");
            }
        });
    }

    private void OnMediaFailed(object? sender, EventArgs e)
    {
        Debug.WriteLine($"=== SignDetailsPage MEDIA FAILED ===");
        Debug.WriteLine($"Source: {SignVideo?.Source}");

        if (SignVideo?.Source is UriMediaSource uriSource)
        {
            Debug.WriteLine($"URI: {uriSource.Uri}");
        }
    }

    private void OnReplayClicked(object? sender, EventArgs e)
    {
        try
        {
            SignVideo.Stop();
            SignVideo.Play();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error replaying video: {ex.Message}");
        }
    }
}
