using com.kizwiz.sipnsign.Models;
using com.kizwiz.sipnsign.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace com.kizwiz.sipnsign.ViewModels
{
    public class ProgressViewModel : INotifyPropertyChanged
    {
        private readonly IProgressService _progressService;
        private UserProgress _userProgress;

        public ObservableCollection<ActivityItem> RecentActivities { get; private set; }
        public ObservableCollection<AchievementItem> Achievements { get; private set; }

        private int _signsLearned;
        public int SignsLearned
        {
            get => _signsLearned;
            set
            {
                if (_signsLearned != value)
                {
                    _signsLearned = value;
                    OnPropertyChanged(nameof(SignsLearned));
                }
            }
        }

        private int _currentStreak;
        public int CurrentStreak
        {
            get => _currentStreak;
            set
            {
                if (_currentStreak != value)
                {
                    _currentStreak = value;
                    OnPropertyChanged(nameof(CurrentStreak));
                }
            }
        }

        private double _accuracy;
        public double Accuracy
        {
            get => _accuracy;
            set
            {
                if (_accuracy != value)
                {
                    _accuracy = value;
                    OnPropertyChanged(nameof(Accuracy));
                    OnPropertyChanged(nameof(AccuracyDisplay));
                }
            }
        }

        public string AccuracyDisplay => $"{Accuracy:P0}";

        private TimeSpan _practiceTime;
        public TimeSpan PracticeTime
        {
            get => _practiceTime;
            set
            {
                if (_practiceTime != value)
                {
                    _practiceTime = value;
                    OnPropertyChanged(nameof(PracticeTime));
                    OnPropertyChanged(nameof(PracticeTimeDisplay));
                }
            }
        }

        public string PracticeTimeDisplay
        {
            get
            {
                if (PracticeTime.TotalHours >= 1)
                    return $"{PracticeTime.TotalHours:F1} hrs";
                return $"{PracticeTime.Minutes} min";
            }
        }

        public ProgressViewModel(IProgressService progressService)
        {
            _progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
            RecentActivities = new ObservableCollection<ActivityItem>();
            Achievements = new ObservableCollection<AchievementItem>();
            // Initialize with default progress
            _userProgress = new UserProgress
            {
                Achievements = new List<Achievement>(),
                Activities = new List<ActivityLog>()
            };
        }

        public async Task LoadProgressAsync()
        {
            _userProgress = await _progressService.GetUserProgressAsync();
            UpdateUI();
        }

        private void UpdateUI()
        {
            SignsLearned = _userProgress.SignsLearned;
            CurrentStreak = _userProgress.CurrentStreak;
            Accuracy = _userProgress.Accuracy;
            PracticeTime = _userProgress.TotalPracticeTime;

            RecentActivities.Clear();
            foreach (var activity in _userProgress.Activities.OrderByDescending(a => a.Timestamp).Take(10))
            {
                var icon = activity.Type == ActivityType.Practice && activity.Score == "+1"
                    ? "quiz_correct_icon"
                    : GetActivityIcon(activity.Type);

                RecentActivities.Add(new ActivityItem
                {
                    Icon = activity.IconName ?? icon,  // Use activity's icon if provided, otherwise use our determined icon
                    Description = activity.Description,
                    TimeAgo = FormatTimeAgo(activity.Timestamp),
                    Score = activity.Score
                });
            }

            Achievements.Clear();
            foreach (var achievement in _userProgress.Achievements.OrderBy(a => a.IsUnlocked))
            {
                var progress = (double)achievement.ProgressCurrent / achievement.ProgressRequired;

                // Choose icon based on unlock status
                var icon = achievement.IsUnlocked ? achievement.Id switch
                {
                    "FIRST_SIGN" => "first_sign_icon",
                    "STREAK_7" => "streak_icon",
                    "STREAK_30" => "streak_icon",
                    "SIGNS_50" => "mastery_icon",
                    "SIGNS_100" => "mastery_icon",
                    "QUIZ_PERFECT" => "quiz_icon",
                    "PRACTICE_HOURS_10" => "time_icon",
                    _ => "achievement_icon"
                } : "locked_icon";

                var iconPath = $"{icon}.svg";

                // Calculate progress text
                var progressText = achievement.Id switch
                {
                    "SIGNS_50" => $"Learn 50 signs ({_userProgress.SignsLearned}/50)",
                    "SIGNS_100" => $"Learn 100 signs ({_userProgress.SignsLearned}/100)",
                    "PRACTICE_HOURS_10" => $"Practice for 10 hours ({(int)_userProgress.TotalPracticeTime.TotalHours}/10)",
                    "STREAK_7" => $"Practice for 7 consecutive days ({_userProgress.CurrentStreak}/7)",
                    "STREAK_30" => $"Practice for 30 consecutive days ({_userProgress.CurrentStreak}/30)",
                    _ => $"{achievement.Description} ({achievement.ProgressCurrent}/{achievement.ProgressRequired})"
                };

                Achievements.Add(new AchievementItem
                {
                    Id = achievement.Id,
                    Icon = icon,
                    Title = achievement.Title,
                    Description = progressText,
                    IsUnlocked = achievement.IsUnlocked,
                    Progress = progress,
                    UnlockedDate = achievement.UnlockedDate
                });
                Debug.WriteLine($"Achievement {achievement.Title} - UnlockedDate: {achievement.UnlockedDate}");
            }
        }

        private string GetActivityIcon(ActivityType type)
        {
            return type switch
            {
                ActivityType.Practice => "quiz_incorrect_icon",  // Default for practice
                ActivityType.Quiz => "quiz_icon",
                ActivityType.Achievement => "achievement_icon",
                ActivityType.Streak => "streak_icon",
                _ => "quiz_icon"
            };
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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public double OverallProgressPercentage
        {
            get
            {
                if (_userProgress?.Achievements == null || !_userProgress.Achievements.Any())
                    return 0;

                double totalProgress = 0;
                var achievementsCount = _userProgress.Achievements.Count;

                foreach (var achievement in _userProgress.Achievements)
                {
                    if (achievement.IsUnlocked)
                    {
                        totalProgress += 1.0;
                    }
                    else if (achievement.ProgressRequired > 0)
                    {
                        var currentProgress = (double)achievement.ProgressCurrent / achievement.ProgressRequired;
                        totalProgress += Math.Min(currentProgress, 1.0); // Cap at 100%
                    }
                }

                return totalProgress / achievementsCount;
            }
        }

        public string OverallProgressText => $"{(OverallProgressPercentage * 100):F0}% Complete";
    }
}
