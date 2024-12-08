using com.kizwiz.sipnsign.Models;
using com.kizwiz.sipnsign.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.kizwiz.sipnsign.ViewModels
{
    public class ScoreboardViewModel : INotifyPropertyChanged
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

        public ScoreboardViewModel(IProgressService progressService)
        {
            _progressService = progressService;
            RecentActivities = new ObservableCollection<ActivityItem>();
            Achievements = new ObservableCollection<AchievementItem>();
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
                RecentActivities.Add(new ActivityItem
                {
                    Icon = GetActivityIcon(activity.Type), // Use the method to get icon
                    Description = activity.Description,
                    TimeAgo = FormatTimeAgo(activity.Timestamp),
                    Score = activity.Score
                });
            }

            Achievements.Clear();
            foreach (var achievement in _userProgress.Achievements.OrderBy(a => a.IsUnlocked))
            {
                var progress = (double)achievement.ProgressCurrent / achievement.ProgressRequired;
                Achievements.Add(new AchievementItem
                {
                    Icon = "achievement_icon.svg",  // Default achievement icon
                    Title = achievement.Title,
                    Description = $"{achievement.Description} ({achievement.ProgressCurrent}/{achievement.ProgressRequired})",
                    IsUnlocked = achievement.IsUnlocked,
                    Progress = progress
                });
            }
        }

        private string GetActivityIcon(ActivityType type)
        {
            return type switch
            {
                ActivityType.Practice => "quiz_icon.svg",
                ActivityType.Quiz => "quiz_icon.svg",
                ActivityType.Achievement => "achievement_icon.svg",
                ActivityType.Streak => "streak_icon.svg",
                _ => "quiz_icon.svg"  // default icon
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
