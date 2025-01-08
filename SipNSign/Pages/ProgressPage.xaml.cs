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

    private async void OnAchievementTapped(object sender, TappedEventArgs e)
    {
        Debug.WriteLine("Achievement tapped!");
        try
        {
            if (sender is VisualElement element)
            {
                Debug.WriteLine($"Sender is VisualElement: {element.BindingContext?.GetType()}");
                if (element.BindingContext is AchievementItem achievementItem)
                {
                    Debug.WriteLine($"Opening details for achievement: {achievementItem.Title}");
                    // Convert AchievementItem to Achievement
                    var achievement = new Achievement
                    {
                        Title = achievementItem.Title,
                        Description = achievementItem.Description,
                        IconName = achievementItem.Icon,
                        IsUnlocked = achievementItem.IsUnlocked,
                        ProgressCurrent = (int)(achievementItem.Progress * 100), // Convert progress to current value
                        ProgressRequired = 100, // Set required progress
                        UnlockedDate = achievementItem.UnlockedDate
                    };

                    var detailsPage = new AchievementDetailsPage(achievement);
                    await Navigation.PushAsync(detailsPage);
                }
                else
                {
                    Debug.WriteLine("BindingContext is not an AchievementItem");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error navigating to achievement details: {ex.Message}");
            await DisplayAlert("Error", "Unable to display achievement details", "OK");
        }
    }
}