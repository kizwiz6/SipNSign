using com.kizwiz.sipnsign.Models;
using com.kizwiz.sipnsign.Services;
using com.kizwiz.sipnsign.ViewModels;
using System.Diagnostics;

namespace com.kizwiz.sipnsign.Pages;

/// <summary>
/// Displays user progress, achievements and activity history
/// </summary>
public partial class ProgressPage : ContentPage
{
    private readonly ProgressViewModel _viewModel;
    private bool _isNavigating = false;

    /// <summary>
    /// Initializes a new instance of ProgressPage
    /// </summary>
    /// <param name="progressService">Service to manage user progress data</param>
    public ProgressPage(IProgressService progressService)
    {
        try
        {
            InitializeComponent();
            _viewModel = new ProgressViewModel(progressService);
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
            await DisplayAlert("Error", "Unable to load progress data", "OK");
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