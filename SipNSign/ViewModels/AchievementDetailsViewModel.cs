using com.kizwiz.sipnsign.Models;
using com.kizwiz.sipnsign.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace com.kizwiz.sipnsign.ViewModels
{
    public class AchievementDetailsViewModel : ObservableObject
    {
        private readonly IShareService _shareService;
        private Achievement _achievement;

        public string Icon => _achievement.IconName;
        public string Title => _achievement.Title;
        public string Description => _achievement.Description;
        public bool IsUnlocked => _achievement.IsUnlocked;
        public double Progress => (double)_achievement.ProgressCurrent / _achievement.ProgressRequired;
        public string ProgressText => $"{_achievement.ProgressCurrent}/{_achievement.ProgressRequired}";

        public string UnlockDateDisplay => _achievement.UnlockedDate.HasValue
            ? $"Unlocked on {_achievement.UnlockedDate.Value:MMM dd, yyyy at hh:mm tt}"
            : string.Empty;

        public ICommand ShareCommand { get; }

        public AchievementDetailsViewModel(Achievement achievement, IShareService shareService)
        {
            _achievement = achievement;
            _shareService = shareService;
            ShareCommand = new AsyncRelayCommand(ShareAchievement);
        }

        private async Task ShareAchievement()
        {
            try
            {
                var shareText = $"I just {(IsUnlocked ? "unlocked" : "made progress on")} the '{Title}' achievement in SipNSign! 🎉\n\n" +
                               $"{Description}\n\n" +
                               $"Download SipNSign and learn sign language while having fun!";

                await _shareService.ShareTextAsync(shareText, "Share Achievement");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sharing achievement: {ex.Message}");
            }
        }
    }

    public class AchievementDetailModel : ObservableObject
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public bool IsUnlocked { get; set; }
        public DateTime? UnlockDate { get; set; }
        public double Progress { get; set; }
        public string ProgressText { get; set; }

        public string UnlockDateDisplay => UnlockDate.HasValue
            ? $"Unlocked on {UnlockDate.Value:MMM dd, yyyy at hh:mm tt}"
            : string.Empty;
    }
}