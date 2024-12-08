using com.kizwiz.sipnsign.Models;
using com.kizwiz.sipnsign.Services;

namespace com.kizwiz.sipnsign.Pages;

public partial class ScoreboardPage : ContentPage
{
    private readonly IProgressService _progressService;
    private UserProgress _userProgress;

    public ScoreboardPage(IProgressService progressService)
    {
        InitializeComponent();
        _progressService = progressService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadUserProgress();
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