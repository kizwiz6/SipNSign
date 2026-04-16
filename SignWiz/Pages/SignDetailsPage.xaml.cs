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

            Loaded += OnPageLoaded;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in SignDetailsPage constructor: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private async void OnPageLoaded(object? sender, EventArgs e)
    {
        Debug.WriteLine("SignDetailsPage: Loaded event fired, loading video...");
        await LoadVideoAsync();
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

            var videoPath = await _videoService.GetVideoPath(videoFileName);
            Debug.WriteLine($"Video path resolved: {videoPath}");

            var source = MediaSource.FromUri(new Uri(videoPath));

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                try
                {
                    SignVideo.Stop();
                    SignVideo.Source = source;
                    _videoSourceSet = true;
                    SignVideo.Play();
                    Debug.WriteLine($"Video source set and Play() called, State: {SignVideo.CurrentState}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error setting video source: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading video: {ex.Message}");
            await DisplayAlert("Error", "Unable to load the sign video.", "OK");
        }
    }

    private void OnMediaOpened(object? sender, EventArgs e)
    {
        Debug.WriteLine($"SignDetailsPage: Media opened, source set: {_videoSourceSet}");
        if (!_videoSourceSet) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                SignVideo.Play();
                Debug.WriteLine($"SignDetailsPage: Play command sent, State: {SignVideo.CurrentState}");
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
