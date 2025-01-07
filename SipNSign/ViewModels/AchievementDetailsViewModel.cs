using com.kizwiz.sipnsign.Models;
using com.kizwiz.sipnsign.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace com.kizwiz.sipnsign.ViewModels
{
    public partial class AchievementDetailsViewModel : ObservableObject
    {
        private readonly INavigation _navigation;
        private readonly IProgressService _progressService;
        private readonly IShareService _shareService;

        [ObservableProperty]
        private ObservableCollection<AchievementDetailModel> achievements;

        [ObservableProperty]
        private AchievementDetailModel currentAchievement;

        public AchievementDetailsViewModel(INavigation navigation,
            IProgressService progressService,
            IShareService shareService,
            Achievement selectedAchievement = null)
        {
            _navigation = navigation;
            _progressService = progressService;
            _shareService = shareService;

            LoadAchievements(selectedAchievement);
        }

        private async void LoadAchievements(Achievement selectedAchievement)
        {
            var progress = await _progressService.GetUserProgressAsync();
            Achievements = new ObservableCollection<AchievementDetailModel>(
                progress.Achievements.Select(a => new AchievementDetailModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    Icon = a.IconName,
                    IsUnlocked = a.IsUnlocked,
                    UnlockDate = a.UnlockedDate,
                    Progress = (double)a.ProgressCurrent / a.ProgressRequired,
                    ProgressText = $"{a.ProgressCurrent}/{a.ProgressRequired}",
                }));

            if (selectedAchievement != null)
            {
                CurrentAchievement = Achievements.FirstOrDefault(a => a.Id == selectedAchievement.Id);
            }
            else
            {
                CurrentAchievement = Achievements.FirstOrDefault();
            }
        }

        [RelayCommand]
        private async Task Share()
        {
            if (CurrentAchievement?.IsUnlocked == true)
            {
                var shareText = $"I just unlocked the {CurrentAchievement.Title} achievement in SipNSign! ??\n" +
                              $"{CurrentAchievement.Description}";

                await _shareService.ShareTextAsync(shareText, "Achievement Unlocked!");
            }
        }

        [RelayCommand]
        private async Task GoBack()
        {
            await _navigation.PopAsync();
        }

        [RelayCommand]
        private void Previous()
        {
            var currentIndex = Achievements.IndexOf(CurrentAchievement);
            if (currentIndex > 0)
            {
                CurrentAchievement = Achievements[currentIndex - 1];
            }
        }

        [RelayCommand]
        private void Next()
        {
            var currentIndex = Achievements.IndexOf(CurrentAchievement);
            if (currentIndex < Achievements.Count - 1)
            {
                CurrentAchievement = Achievements[currentIndex + 1];
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