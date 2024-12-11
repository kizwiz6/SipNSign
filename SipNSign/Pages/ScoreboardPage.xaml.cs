using com.kizwiz.sipnsign.Models;
using com.kizwiz.sipnsign.Services;
using com.kizwiz.sipnsign.ViewModels;
using System.Diagnostics;

namespace com.kizwiz.sipnsign.Pages;

public partial class ScoreboardPage : ContentPage
{
    private readonly IProgressService _progressService;
    private UserProgress _userProgress;
    private readonly ScoreboardViewModel _viewModel;

    public ScoreboardPage(IProgressService progressService)
    {
        try
        {
            InitializeComponent();
            _viewModel = new ScoreboardViewModel(progressService);
            BindingContext = _viewModel;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in ScoreboardPage constructor: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            throw; // Rethrow to be caught by the calling method
        }
    }

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

    private async Task LoadUserProgress()
    {
        _userProgress = await _progressService.GetUserProgressAsync();
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Update statistics
        SignsLearnedLabel.Text = _userProgress.SignsLearned.ToString();
        StreakLabel.Text = $"{_userProgress.CurrentStreak} Days";
        AccuracyLabel.Text = $"{_userProgress.Accuracy:P0}";
        PracticeTimeLabel.Text = FormatPracticeTime(_userProgress.TotalPracticeTime);

        // Update progress bar
        var totalSigns = 100; // Total signs available in the app
        OverallProgressBar.Progress = (double)_userProgress.SignsLearned / totalSigns;
        ProgressLabel.Text = $"{_userProgress.SignsLearned}/{totalSigns} Signs";

        // Update recent activities
        RecentActivityList.ItemsSource = _userProgress.Activities
            .OrderByDescending(a => a.Timestamp)
            .Take(10)
            .Select(a => new ActivityItem
            {
                Icon = a.IconName,
                Description = a.Description,
                TimeAgo = FormatTimeAgo(a.Timestamp),
                Score = a.Score
            });

        // Update achievements
        AchievementsList.ItemsSource = _userProgress.Achievements
            .Select(a => new AchievementItem
            {
                Icon = a.IconName,
                Title = a.Title,
                Description = $"{a.Description} ({a.ProgressCurrent}/{a.ProgressRequired})",
                IsUnlocked = a.IsUnlocked
            });
    }

    private string FormatPracticeTime(TimeSpan time)
    {
        if (time.TotalHours >= 1)
            return $"{time.TotalHours:F1} hrs";
        return $"{time.Minutes} min";
    }

    private string FormatTimeAgo(DateTime timestamp)
    {
        var timeSpan = DateTime.Now - timestamp;

        if (timeSpan.TotalMinutes < 1)
            return "Just now";
        if (timeSpan.TotalHours < 1)
            return $"{timeSpan.Minutes}m ago";
        if (timeSpan.TotalDays < 1)
            return $"{timeSpan.Hours}h ago";
        if (timeSpan.TotalDays < 7)
            return $"{timeSpan.Days}d ago";
        return timestamp.ToString("MMM dd");
    }
}