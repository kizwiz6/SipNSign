using com.kizwiz.signwiz.Models;
using com.kizwiz.signwiz.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Windows.Input;

namespace com.kizwiz.signwiz.ViewModels
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

        /// <summary>
        /// Delegate set by the page to capture the achievement card as an image stream.
        /// </summary>
        public Func<Task<Stream?>>? CaptureCardAsync { get; set; }
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
                // Try to capture and share the card as an image
                if (CaptureCardAsync != null)
                {
                    var stream = await CaptureCardAsync();
                    if (stream != null)
                    {
                        var filePath = Path.Combine(FileSystem.CacheDirectory, "achievement_share.png");
                        using (var fileStream = File.Create(filePath))
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                        stream.Dispose();

                        await _shareService.ShareImageAsync(filePath, "Check out my achievement in SignWiz! 🧙‍♂️✨");
                        await LogShareActivity();
                        return;
                    }
                }

                // Fallback to text sharing
                var shareText = $"🎮 Achievement Unlocked in SignWiz! 🎉\n\n" +
                               $"🏆 {Title}\n" +
                               $"📝 {Description}\n\n" +
                               $"🗓️ Unlocked on: {_achievement.UnlockedDate:dd MMM yyyy}\n\n" +
                               $"🎯 Progress: {_achievement.ProgressCurrent}/{_achievement.ProgressRequired}\n\n" +
                               $"🧙‍♂️ Download SignWiz and learn sign language while having fun!\n" +
                               $"#SignWiz #SignLanguage #Gaming";

                await _shareService.ShareTextAsync(shareText, "Share Achievement");
                await LogShareActivity();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sharing achievement: {ex.Message}");
            }
        }

        private async Task LogShareActivity()
        {
            // Log the sharing activity
            await _progressService.LogActivityAsync(new ActivityLog
            {
                Id = Guid.NewGuid().ToString(),
                Type = ActivityType.Achievement,
                Description = "Shared an achievement",
                IconName = "social_icon",
                Timestamp = DateTime.Now,
                Score = "🏆"
            });

            // Find and unlock the Social Butterfly achievement
            var userProgress = await _progressService.GetUserProgressAsync();
            var socialAchievement = userProgress.Achievements
                .FirstOrDefault(a => a.Id == "SOCIAL_BUTTERFLY");

            if (socialAchievement != null && !socialAchievement.IsUnlocked)
            {
                socialAchievement.ProgressCurrent = socialAchievement.ProgressRequired;
                socialAchievement.IsUnlocked = true;
                socialAchievement.UnlockedDate = DateTime.Now;

                await _progressService.SaveProgressAsync(userProgress);
                await _progressService.UpdateAchievementsAsync();
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