using com.kizwiz.signwiz.Models;
using com.kizwiz.signwiz.Services;
using com.kizwiz.signwiz.ViewModels;
using System.Diagnostics;

namespace com.kizwiz.signwiz.Pages;

/// <summary>
/// Displays user progress, achievements and activity history
/// </summary>
public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel;
    private bool _isNavigating = false;

    /// <summary>
    /// Initializes a new instance of ProgressPage
    /// </summary>
    /// <param name="progressService">Service to manage user progress data</param>
    public ProfilePage(IProgressService progressService)
    {
        try
        {
            InitializeComponent();
            _viewModel = new ProfileViewModel(progressService);
            BindingContext = _viewModel;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in ProgressPage constructor: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Loads progress data when the page appears
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _viewModel.LoadProgressAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading progress: {ex.Message}");
            await DisplayAlertAsync("Error", "Unable to load progress data", "OK");
        }
    }

    /// <summary>
    /// Event handler for activity tap events. Navigates to SignDetailsPage if the activity has an associated sign.
    /// </summary>
    private async void OnActivityTapped(object? sender, TappedEventArgs e)
    {
        if (_isNavigating) return;

        try
        {
            _isNavigating = true;

            if (sender is VisualElement element &&
                element.BindingContext is ActivityItem activityItem)
            {
                if (string.IsNullOrEmpty(activityItem.SignName))
                {
                    Debug.WriteLine("Activity has no associated sign - ignoring tap");
                    return;
                }

                var services = Application.Current?.Handler?.MauiContext?.Services;
                var signRepository = services?.GetService<SignRepository>();

                if (signRepository == null)
                {
                    Debug.WriteLine("SignRepository not available");
                    await DisplayAlert("Error", "Unable to load sign data.", "OK");
                    return;
                }

                var sign = signRepository.GetSignByName(activityItem.SignName);

                if (sign == null)
                {
                    Debug.WriteLine($"Sign not found for name: {activityItem.SignName}");
                    await DisplayAlert("Error", "Sign data not found.", "OK");
                    return;
                }

                var detailsPage = new SignDetailsPage(sign);
                await Navigation.PushAsync(detailsPage);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnActivityTapped: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            await DisplayAlert("Error", "Unable to display sign details.", "OK");
        }
        finally
        {
            _isNavigating = false;
        }
    }

    /// <summary>
    /// Event handler for achievement tap events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments.</param>
    private async void OnAchievementTapped(object? sender, TappedEventArgs e)
    {
        if (_isNavigating) return; // Prevent multiple navigations

        Debug.WriteLine("Achievement tapped!");
        try
        {
            _isNavigating = true; // Set this right after checking to prevent race conditions

            if (sender is VisualElement element &&
                element.BindingContext is AchievementItem achievementItem)
            {
                Debug.WriteLine($"Achievement data: Title={achievementItem.Title}, IsUnlocked={achievementItem.IsUnlocked}, UnlockDate={achievementItem.UnlockedDate}");

                // Create achievement with all required data
                var achievement = new Achievement
                {
                    Id = achievementItem.Id ?? "unknown",
                    Title = achievementItem.Title,
                    Description = achievementItem.Description,
                    IconName = achievementItem.Icon,
                    IsUnlocked = achievementItem.IsUnlocked,
                    UnlockedDate = achievementItem.UnlockedDate,
                    ProgressCurrent = (int)(achievementItem.Progress * 100),
                    ProgressRequired = 100
                };

                try
                {
                    var detailsPage = new AchievementDetailsPage(achievement);
                    await Navigation.PushAsync(detailsPage);
                }
                catch (Exception pageEx)
                {
                    Debug.WriteLine($"Error creating details page: {pageEx}");
                    throw;
                }
            }
            else
            {
                Debug.WriteLine("BindingContext is not an AchievementItem");
                await DisplayAlert("Error", "Achievement data not found", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnAchievementTapped: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            await DisplayAlert("Error", "Unable to display achievement details. Please ensure you're connected to the internet.", "OK");
        }
        finally
        {
            _isNavigating = false;
        }
    }
}