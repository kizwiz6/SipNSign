using com.kizwiz.signwiz.Models;
using com.kizwiz.signwiz.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
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
        public string Icon => _achievement.IsUnlocked ? _achievement.IconName : "locked_icon";
        public string Title => _achievement.Title;
        public string Description => _achievement.Description;
        public bool IsUnlocked => _achievement.IsUnlocked;
        public double Progress => _achievement.ProgressRequired > 0
            ? (double)_achievement.ProgressCurrent / _achievement.ProgressRequired
            : 0;
        public string ProgressText => $"{_achievement.ProgressCurrent}/{_achievement.ProgressRequired}";

        public string UnlockDateDisplay => _achievement.UnlockedDate.HasValue
            ? $"Unlocked on {_achievement.UnlockedDate.Value:MMM dd, yyyy 'at' hh:mm tt}"
            : string.Empty;

        public ICommand ShareCommand { get; }

        /// <summary>
        /// Delegate set by the page to capture the achievement card as an image stream.
        /// </summary>
        public Func<Task<Stream?>>? CaptureCardAsync { get; set; }

        /// <summary>
        /// Collection of all achievement thumbnails for the bottom carousel.
        /// </summary>
        public ObservableCollection<AchievementThumbnail> AchievementCollection { get; } = new();

        private string _achievementsCountText = string.Empty;
        /// <summary>
        /// Display text for unlocked achievement count (e.g., "Achievements Unlocked: 2/25").
        /// </summary>
        public string AchievementsCountText
        {
            get => _achievementsCountText;
            private set => SetProperty(ref _achievementsCountText, value);
        }
        #endregion

        #region Constructor
        public AchievementDetailsViewModel(Achievement achievement, IShareService shareService, IProgressService progressService)
        {
            _achievement = achievement;
            _shareService = shareService;
            _progressService = progressService;
            ShareCommand = new AsyncRelayCommand(ShareAchievement);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Loads all achievements into the carousel collection.
        /// </summary>
        public async Task LoadAchievementCollectionAsync()
        {
            try
            {
                var userProgress = await _progressService.GetUserProgressAsync();
                _userProgress = userProgress;

                var unlockedCount = userProgress.Achievements.Count(a => a.IsUnlocked);
                var totalCount = userProgress.Achievements.Count;
                AchievementsCountText = $"Achievements Unlocked: {unlockedCount}/{totalCount}";

                AchievementCollection.Clear();
                foreach (var ach in userProgress.Achievements)
                {
                    var icon = GetAchievementIcon(ach);
                    AchievementCollection.Add(new AchievementThumbnail(ach, icon, ach.Id == _achievement.Id));
                }

                // Sync main card to the UserProgress version for consistent data
                var match = userProgress.Achievements.FirstOrDefault(a => a.Id == _achievement.Id);
                if (match != null)
                {
                    _achievement = match;
                    NotifyAllCardProperties();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading achievement collection: {ex.Message}");
            }
        }

        /// <summary>
        /// Selects a different achievement from the carousel, updating the main card.
        /// </summary>
        public void SelectAchievement(AchievementThumbnail thumbnail)
        {
            if (thumbnail == null) return;

            _achievement = thumbnail.Achievement;

            // Update selection state
            foreach (var item in AchievementCollection)
            {
                item.IsSelected = item.Achievement.Id == _achievement.Id;
            }

            NotifyAllCardProperties();
        }
        #endregion

        #region Private Methods
        private void NotifyAllCardProperties()
        {
            OnPropertyChanged(nameof(Icon));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(IsUnlocked));
            OnPropertyChanged(nameof(Progress));
            OnPropertyChanged(nameof(ProgressText));
            OnPropertyChanged(nameof(UnlockDateDisplay));
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
                        await TryUnlockSocialButterfly();
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
                await TryUnlockSocialButterfly();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sharing achievement: {ex.Message}");
            }
        }

        /// <summary>
        /// Unlocks the Social Butterfly achievement on first share.
        /// Does not log generic "Shared an achievement" activity for every share.
        /// </summary>
        private async Task TryUnlockSocialButterfly()
        {
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

                // Log only the Social Butterfly unlock itself
                await _progressService.LogActivityAsync(new ActivityLog
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = ActivityType.Achievement,
                    Description = "Unlocked 'Social Butterfly' achievement",
                    IconName = "social_icon",
                    Timestamp = DateTime.Now,
                    Score = "🏆"
                });

                // Refresh the carousel to reflect the newly unlocked achievement
                await LoadAchievementCollectionAsync();
            }
        }

        private static string GetAchievementIcon(Achievement achievement)
        {
            if (!achievement.IsUnlocked) return "locked_icon";

            return achievement.Id switch
            {
                "FIRST_SIGN" => "first_sign_icon",
                "STREAK_7" => "streak_weekly_icon",
                "STREAK_30" => "streak_monthly_icon",
                "SIGNS_50" => "fifty_signs_icon",
                "SIGNS_100" => "century_club_icon",
                "QUIZ_PERFECT" => "quiz_master_icon",
                "PRACTICE_HOURS_10" => "time_icon",
                "SIGNS_100_GUESS" => "guess_100_icon",
                "SIGNS_1000_GUESS" => "guess_1000_icon",
                "SIGNS_100_PERFORM" => "perform_100_icon",
                "SIGNS_1000_PERFORM" => "perform_1000_icon",
                "PERFECT_SESSION" => "perfect_session_icon",
                "SOCIAL_BUTTERFLY" => "social_icon",
                "RAPID_FIRE" => "speed_icon",
                "SPEED_MASTER" => "speed_master_icon",
                "PARTY_HOST" => "party_icon",
                "MULTIPLAYER_FIRST" => "multiplayer_icon",
                "MULTIPLAYER_10" => "multiplayer_10_icon",
                "MULTIPLAYER_50" => "multiplayer_50_icon",
                "CHAMPION_WIN" => "champion_icon",
                "CLOSE_CALL" => "close_call_icon",
                "PERFECT_ROUND_ALL" => "harmony_icon",
                "COMEBACK_KING" => "comeback_icon",
                "PERFECT_MULTIPLAYER" => "perfect_multi_icon",
                "PARTY_ANIMAL" => "party_animal_icon",
                _ => "achievement_icon"
            };
        }
        #endregion
    }

    /// <summary>
    /// Represents a single achievement thumbnail in the carousel collection.
    /// </summary>
    public class AchievementThumbnail : ObservableObject
    {
        public Achievement Achievement { get; }
        public string Icon { get; }
        public bool IsUnlocked { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public AchievementThumbnail(Achievement achievement, string icon, bool isSelected)
        {
            Achievement = achievement;
            Icon = icon;
            IsUnlocked = achievement.IsUnlocked;
            _isSelected = isSelected;
        }
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