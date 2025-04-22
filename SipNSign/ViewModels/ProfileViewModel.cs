using com.kizwiz.sipnsign.Models;
using com.kizwiz.sipnsign.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace com.kizwiz.sipnsign.ViewModels
{
    /// <summary>
    /// View model for managing user progress and achievements display
    /// </summary>
    public class ProfileViewModel : INotifyPropertyChanged
    {
        #region Fields
        private readonly IProgressService _progressService;
        private UserProgress _userProgress;
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

        private int _guessModeCount;
        public int GuessModeSigns
        {
            get => _guessModeCount;
            set
            {
                if (_guessModeCount != value)
                {
                    _guessModeCount = value;
                    OnPropertyChanged(nameof(GuessModeSigns));
                }
            }
        }

        private int _performModeCount;
        public int PerformModeSigns
        {
            get => _performModeCount;
            set
            {
                if (_performModeCount != value)
                {
                    _performModeCount = value;
                    OnPropertyChanged(nameof(PerformModeSigns));
                }
            }
        }
        #endregion

        #region Properties
        public ObservableCollection<ActivityItem> RecentActivities { get; private set; }
        public ObservableCollection<AchievementItem> Achievements { get; private set; }
        public string AchievementsHeaderText => $"Achievements ({_userProgress.Achievements.Count(a => a.IsUnlocked)}/{_userProgress.Achievements.Count})";
        #endregion


        public string AccuracyDisplay => $"{Accuracy:P0}";

        

        public string PracticeTimeDisplay
        {
            get
            {
                if (PracticeTime.TotalHours >= 1)
                    return $"{PracticeTime.TotalHours:F1} hrs";
                return $"{PracticeTime.Minutes} min";
            }
        }

        #region Constructor
        public ProfileViewModel(IProgressService progressService)
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
        #endregion

        #region Public Methods
        /// <summary>
        /// Loads and updates user progress data
        /// </summary>
        public async Task LoadProgressAsync()
        {
            _userProgress = await _progressService.GetUserProgressAsync();
            UpdateUI();
        }
        #endregion

        #region Private Methods
        private void UpdateUI()
        {
            SignsLearned = _userProgress.SignsLearned;
            CurrentStreak = _userProgress.CurrentStreak;
            Accuracy = _userProgress.Accuracy;
            PracticeTime = _userProgress.TotalPracticeTime;
            GuessModeSigns = _userProgress.GuessModeSigns;
            PerformModeSigns = _userProgress.PerformModeSigns;

            // Update RecentActivities
            RecentActivities.Clear();
            foreach (var activity in _userProgress.Activities
                .Where(a => !string.IsNullOrEmpty(a.Description)) // Filter out empty descriptions
                .OrderByDescending(a => a.Timestamp)
                .Take(10))
            {
                var icon = activity.Type switch
                {
                    ActivityType.Achievement => "achievement_icon",
                    ActivityType.Practice when activity.Score == "+1" => "quiz_correct_icon",
                    ActivityType.Practice => "quiz_incorrect_icon",
                    ActivityType.Quiz => "quiz_icon",
                    ActivityType.Streak => "streak_icon",
                    _ => "quiz_icon"
                };

                var score = activity.Type == ActivityType.Achievement ? "🏆" : activity.Score;

                RecentActivities.Add(new ActivityItem
                {
                    Icon = activity.IconName ?? icon,
                    Description = activity.Description ?? "Activity recorded", // Provide fallback
                    TimeAgo = FormatTimeAgo(activity.Timestamp),
                    Score = score ?? "" // Provide fallback for score
                });
            }

            OnPropertyChanged(nameof(AchievementsHeaderText));

            // Update Achievements
            Achievements.Clear();
            foreach (var achievement in _userProgress.Achievements.OrderBy(a => a.IsUnlocked))
            {
                var progress = (double)achievement.ProgressCurrent / achievement.ProgressRequired;

                // Choose icon based on unlock status
                var icon = achievement.IsUnlocked ? achievement.Id switch
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
                    _ => "achievement_icon"
                } : "locked_icon";

                var iconPath = $"{icon}.svg";

                // Calculate progress text only if achievement is not unlocked
                var progressText = !achievement.IsUnlocked ? achievement.Id switch
                {
                    "SIGNS_50" => $"Learn 50 signs ({Math.Min(_userProgress.SignsLearned, 50)}/50)",
                    "SIGNS_100" => $"Learn 100 signs ({Math.Min(_userProgress.SignsLearned, 100)}/100)",
                    "PRACTICE_HOURS_10" => $"Practice for 10 hours ({Math.Min((int)_userProgress.TotalPracticeTime.TotalHours, 10)}/10)",
                    "STREAK_7" => $"Practice for 7 consecutive days ({Math.Min(_userProgress.CurrentStreak, 7)}/7)",
                    "STREAK_30" => $"Practice for 30 consecutive days ({Math.Min(_userProgress.CurrentStreak, 30)}/30)",
                    _ => $"{achievement.Description} ({Math.Min(achievement.ProgressCurrent, achievement.ProgressRequired)}/{achievement.ProgressRequired})"
                } : achievement.Description;

                Achievements.Add(new AchievementItem
                {
                    Id = achievement.Id,
                    Icon = icon,
                    Title = achievement.Title,
                    Description = progressText,
                    IsUnlocked = achievement.IsUnlocked,
                    Progress = achievement.IsUnlocked ? 1.0 : 0.0,
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
        #endregion

        #region Event Handlers
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
