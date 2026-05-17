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
    /// Event handler for activity selection in CollectionView. Navigates to SignDetailsPage if the activity has an associated sign,
    /// or to AchievementDetailsPage if it's an achievement unlock activity.
    /// </summary>
    private async void OnActivitySelected(object? sender, SelectionChangedEventArgs e)
    {
        Debug.WriteLine("=== OnActivitySelected fired ===");

        if (_isNavigating)
        {
            Debug.WriteLine("Already navigating, ignoring selection");
            return;
        }

        try
        {
            _isNavigating = true;

            // Get the selected item
            var selectedItem = e.CurrentSelection.FirstOrDefault() as ActivityItem;

            if (selectedItem == null)
            {
                Debug.WriteLine("No item selected");
                return;
            }

            Debug.WriteLine($"Selected ActivityItem: Description={selectedItem.Description}, AchievementId={selectedItem.AchievementId}, SignName={selectedItem.SignName}");

            // Clear selection immediately for better UX
            if (sender is CollectionView collectionView)
            {
                collectionView.SelectedItem = null;
            }

            // Check if this is an achievement activity
            if (!string.IsNullOrEmpty(selectedItem.AchievementId))
            {
                Debug.WriteLine($"Navigating to achievement: {selectedItem.AchievementId}");

                var services = Application.Current?.Handler?.MauiContext?.Services;
                var progressService = services?.GetService<IProgressService>();

                if (progressService == null)
                {
                    Debug.WriteLine("ProgressService not available");
                    await DisplayAlertAsync("Error", "Unable to load achievement data.", "OK");
                    return;
                }

                var userProgress = await progressService.GetUserProgressAsync();
                var achievement = userProgress.Achievements.FirstOrDefault(a => a.Id == selectedItem.AchievementId);

                if (achievement == null)
                {
                    Debug.WriteLine($"Achievement not found for ID: {selectedItem.AchievementId}");
                    await DisplayAlertAsync("Error", "Achievement not found.", "OK");
                    return;
                }

                var detailsPage = new AchievementDetailsPage(achievement);
                await Navigation.PushAsync(detailsPage);
                return;
            }

            // Fallback: Try to parse achievement from description for legacy activities
            if (selectedItem.Description?.Contains("Achievement Unlocked:") == true ||
                selectedItem.Description?.Contains("Unlocked '") == true)
            {
                Debug.WriteLine("Legacy achievement activity detected - trying to match by title");

                var services = Application.Current?.Handler?.MauiContext?.Services;
                var progressService = services?.GetService<IProgressService>();

                if (progressService == null)
                {
                    Debug.WriteLine("ProgressService not available");
                    await DisplayAlertAsync("Error", "Unable to load achievement data.", "OK");
                    return;
                }

                var userProgress = await progressService.GetUserProgressAsync();

                // Try to extract achievement title from description
                string? achievementTitle = null;
                if (selectedItem.Description.Contains("Achievement Unlocked:"))
                {
                    achievementTitle = selectedItem.Description.Replace("Achievement Unlocked:", "").Trim();
                }
                else if (selectedItem.Description.Contains("Unlocked '"))
                {
                    var startIdx = selectedItem.Description.IndexOf("'") + 1;
                    var endIdx = selectedItem.Description.IndexOf("'", startIdx);
                    if (startIdx > 0 && endIdx > startIdx)
                    {
                        achievementTitle = selectedItem.Description.Substring(startIdx, endIdx - startIdx);
                    }
                }

                Debug.WriteLine($"Parsed achievement title: {achievementTitle}");

                if (!string.IsNullOrEmpty(achievementTitle))
                {
                    var achievement = userProgress.Achievements.FirstOrDefault(a => 
                        a.Title.Equals(achievementTitle, StringComparison.OrdinalIgnoreCase));

                    if (achievement != null)
                    {
                        Debug.WriteLine($"Found achievement by title: {achievement.Id}");
                        var detailsPage = new AchievementDetailsPage(achievement);
                        await Navigation.PushAsync(detailsPage);
                        return;
                    }
                    else
                    {
                        Debug.WriteLine($"No achievement found matching title: {achievementTitle}");
                    }
                }
            }

            // Otherwise, check if it's a sign activity
            if (!string.IsNullOrEmpty(selectedItem.SignName))
            {
                Debug.WriteLine($"Navigating to sign: {selectedItem.SignName}");

                var services = Application.Current?.Handler?.MauiContext?.Services;
                var signRepository = services?.GetService<SignRepository>();

                if (signRepository == null)
                {
                    Debug.WriteLine("SignRepository not available");
                    await DisplayAlertAsync("Error", "Unable to load sign data.", "OK");
                    return;
                }

                var sign = signRepository.GetSignByName(selectedItem.SignName);

                if (sign == null)
                {
                    Debug.WriteLine($"Sign not found for name: {selectedItem.SignName}");
                    await DisplayAlertAsync("Error", "Sign data not found.", "OK");
                    return;
                }

                var signDetailsPage = new SignDetailsPage(sign);
                await Navigation.PushAsync(signDetailsPage);
                return;
            }

            Debug.WriteLine("Activity has no associated sign or achievement - ignoring selection");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnActivitySelected: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            await DisplayAlertAsync("Error", "Unable to display details.", "OK");
        }
        finally
        {
            _isNavigating = false;
        }
    }

    /// <summary>
    /// Legacy event handler for activity tap events (kept for backwards compatibility).
    /// </summary>
    private async void OnActivityTapped(object? sender, TappedEventArgs e)
    {
        Debug.WriteLine("=== OnActivityTapped fired ===");

        if (_isNavigating)
        {
            Debug.WriteLine("Already navigating, ignoring tap");
            return;
        }

        try
        {
            _isNavigating = true;

            Debug.WriteLine($"Sender type: {sender?.GetType().Name}");

            if (sender is VisualElement element)
            {
                Debug.WriteLine($"BindingContext type: {element.BindingContext?.GetType().Name}");

                if (element.BindingContext is ActivityItem activityItem)
                {
                    Debug.WriteLine($"ActivityItem: Description={activityItem.Description}, AchievementId={activityItem.AchievementId}, SignName={activityItem.SignName}");

                    // Check if this is an achievement activity
                    if (!string.IsNullOrEmpty(activityItem.AchievementId))
                    {
                        Debug.WriteLine($"Achievement activity tapped: {activityItem.AchievementId}");

                    var services = Application.Current?.Handler?.MauiContext?.Services;
                    var progressService = services?.GetService<IProgressService>();

                    if (progressService == null)
                    {
                        Debug.WriteLine("ProgressService not available");
                        await DisplayAlertAsync("Error", "Unable to load achievement data.", "OK");
                        return;
                    }

                    var userProgress = await progressService.GetUserProgressAsync();
                    var achievement = userProgress.Achievements.FirstOrDefault(a => a.Id == activityItem.AchievementId);

                    if (achievement == null)
                    {
                        Debug.WriteLine($"Achievement not found for ID: {activityItem.AchievementId}");
                        await DisplayAlertAsync("Error", "Achievement not found.", "OK");
                        return;
                    }

                    var detailsPage = new AchievementDetailsPage(achievement);
                    await Navigation.PushAsync(detailsPage);
                    return;
                }

                            // Otherwise, check if it's a sign activity
                            if (string.IsNullOrEmpty(activityItem.SignName))
                            {
                                Debug.WriteLine("Activity has no associated sign or achievement - ignoring tap");
                                return;
                            }

                            var services2 = Application.Current?.Handler?.MauiContext?.Services;
                            var signRepository = services2?.GetService<SignRepository>();

                            if (signRepository == null)
                            {
                                Debug.WriteLine("SignRepository not available");
                                await DisplayAlertAsync("Error", "Unable to load sign data.", "OK");
                                return;
                            }

                            var sign = signRepository.GetSignByName(activityItem.SignName);

                            if (sign == null)
                            {
                                Debug.WriteLine($"Sign not found for name: {activityItem.SignName}");
                                await DisplayAlertAsync("Error", "Sign data not found.", "OK");
                                return;
                            }

                            var signDetailsPage = new SignDetailsPage(sign);
                            await Navigation.PushAsync(signDetailsPage);
                        }
                        else
                        {
                            Debug.WriteLine("BindingContext is not ActivityItem");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Sender is not VisualElement");
                    }
                }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnActivityTapped: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            await DisplayAlertAsync("Error", "Unable to display details.", "OK");
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
                await DisplayAlertAsync("Error", "Achievement data not found", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnAchievementTapped: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            await DisplayAlertAsync("Error", "Unable to display achievement details. Please ensure you're connected to the internet.", "OK");
        }
        finally
        {
            _isNavigating = false;
        }
    }
}