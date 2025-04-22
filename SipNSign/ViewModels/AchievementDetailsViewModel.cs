using com.kizwiz.sipnsign.Models;
using com.kizwiz.sipnsign.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Windows.Input;

namespace com.kizwiz.sipnsign.ViewModels
{
    /// <summary>
    /// View model for displaying achievement details
    /// </summary>
    public class AchievementDetailsViewModel : ObservableObject
    {
        #region Fields
        private readonly IShareService _shareService;
        private readonly IProgressService _progressService;
        private Achievement _achievement;
        private UserProgress? _userProgress;
        #endregion

        #region Properties
        public string Icon => _achievement.IconName;
        public string Title => _achievement.Title;
        public string Description => _achievement.Description;
        public bool IsUnlocked => _achievement.IsUnlocked;
        public double Progress => (double)_achievement.ProgressCurrent / _achievement.ProgressRequired;
        public string ProgressText => $"{_achievement.ProgressCurrent}/{_achievement.ProgressRequired}";

        public string UnlockDateDisplay => _achievement.UnlockedDate.HasValue
            ? $"Unlocked on {_achievement.UnlockedDate.Value:MMM dd, yyyy 'at' hh:mm tt}"
            : string.Empty;

        public ICommand ShareCommand { get; }
        #endregion

        #region Constructor
        public AchievementDetailsViewModel(Achievement achievement, IShareService shareService, IProgressService progressService)
        {
            _achievement = achievement;
            _shareService = shareService;
            _progressService = progressService;
            ShareCommand = new AsyncRelayCommand(ShareAchievement);

            // Initialize user progress
            InitializeUserProgress();
        }
        #endregion

        #region Private Methods
        private async void InitializeUserProgress()
        {
            try
            {
                _userProgress = await _progressService.GetUserProgressAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing user progress: {ex.Message}");
            }
        }
        private async Task ShareAchievement()
        {
            try
            {
                var shareText = $"🎮 Achievement Unlocked in SipNSign! 🎉\n\n" +
                               $"🏆 {Title}\n" +
                               $"📝 {Description}\n\n" +
                               $"🗓️ Unlocked on: {_achievement.UnlockedDate:dd MMM yyyy}\n\n" +
                               $"🎯 Progress: {_achievement.ProgressCurrent}/{_achievement.ProgressRequired}\n\n" +
                               $"🎲 Download SipNSign and learn sign language while having fun!\n" +
                               $"#SipNSign #SignLanguage #Gaming";

                await _shareService.ShareTextAsync(shareText, "Share Achievement");

                // Log the sharing activity
                await _progressService.LogActivityAsync(new ActivityLog
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = ActivityType.Achievement,
                    Description = "Shared an achievement",
                    IconName = "social_icon",
                    Timestamp = DateTime.Now,
                    Score = "🏆"  // Use trophy emoji for score
                });

                // Find and unlock the Social Butterfly achievement
                var userProgress = await _progressService.GetUserProgressAsync();
                var socialAchievement = userProgress.Achievements
                    .FirstOrDefault(a => a.Id == "SOCIAL_BUTTERFLY");

                if (socialAchievement != null && !socialAchievement.IsUnlocked)
                {
                    // Force set progress and unlock status
                    socialAchievement.ProgressCurrent = socialAchievement.ProgressRequired;
                    socialAchievement.IsUnlocked = true;
                    socialAchievement.UnlockedDate = DateTime.Now;

                    // Save progress
                    await _progressService.SaveProgressAsync(userProgress);

                    // Force update achievements
                    await _progressService.UpdateAchievementsAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sharing achievement: {ex.Message}");
            }
        }
        #endregion
    }

    public class AchievementDetailModel : ObservableObject
    {
        public required string Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Icon { get; set; }
        public bool IsUnlocked { get; set; }
        public DateTime? UnlockDate { get; set; }
        public double Progress { get; set; }
        public string ProgressText { get; set; }

        public string UnlockDateDisplay => UnlockDate.HasValue
            ? UnlockDate.Value.ToString("dd MMMM yyyy 'at' hh:mm tt")
            : string.Empty;
    }
}