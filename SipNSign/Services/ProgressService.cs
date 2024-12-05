using com.kizwiz.sipnsign.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace com.kizwiz.sipnsign.Services
{
    public class ProgressService : IProgressService
    {
        private readonly string _progressFile;
        private UserProgress _currentProgress;
        private readonly List<Achievement> _availableAchievements;

        public ProgressService()
        {
            _progressFile = Path.Combine(FileSystem.AppDataDirectory, "progress.json");
            _availableAchievements = InitializeAchievements();
            _currentProgress = LoadProgress().Result;
        }

        private List<Achievement> InitializeAchievements()
        {
            return new List<Achievement>
            {
                new Achievement
                {
                    Id = "STREAK_7",
                    Title = "Week Warrior",
                    Description = "Practice for 7 consecutive days",
                    IconName = "streak_icon",
                    ProgressRequired = 7
                },
                new Achievement
                {
                    Id = "SIGNS_50",
                    Title = "Sign Master",
                    Description = "Learn 50 signs",
                    IconName = "practice_icon",
                    ProgressRequired = 50
                },
                new Achievement
                {
                    Id = "QUIZ_PERFECT",
                    Title = "Perfect Score",
                    Description = "Get 100% on a quiz",
                    IconName = "quiz_icon",
                    ProgressRequired = 1
                }
            };
        }

        private async Task<UserProgress> LoadProgress()
        {
            if (!File.Exists(_progressFile))
            {
                return new UserProgress
                {
                    SignsLearned = 0,
                    CurrentStreak = 0,
                    Accuracy = 0,
                    TotalPracticeTime = TimeSpan.Zero,
                    Achievements = _availableAchievements,
                    Activities = new List<ActivityLog>()
                };
            }

            var json = await File.ReadAllTextAsync(_progressFile);
            return JsonSerializer.Deserialize<UserProgress>(json);
        }

        public async Task<UserProgress> GetUserProgressAsync()
        {
            return _currentProgress;
        }

        public async Task LogActivityAsync(ActivityLog activity)
        {
            _currentProgress.Activities.Insert(0, activity);

            // Keep only last 100 activities
            if (_currentProgress.Activities.Count > 100)
                _currentProgress.Activities = _currentProgress.Activities.Take(100).ToList();

            // Update statistics based on activity
            UpdateStatistics(activity);

            await SaveProgressAsync(_currentProgress);
            await UpdateAchievementsAsync();
        }

        private void UpdateStatistics(ActivityLog activity)
        {
            switch (activity.Type)
            {
                case ActivityType.Practice:
                    _currentProgress.SignsLearned++;
                    break;
                case ActivityType.Quiz:
                    // Parse score (e.g., "8/10") and update accuracy
                    var scoreParts = activity.Score.Split('/');
                    if (scoreParts.Length == 2)
                    {
                        double correct = double.Parse(scoreParts[0]);
                        double total = double.Parse(scoreParts[1]);
                        _currentProgress.Accuracy =
                            (_currentProgress.Accuracy * (_currentProgress.SignsLearned - 1) + (correct / total))
                            / _currentProgress.SignsLearned;
                    }
                    break;
            }
        }

        public async Task UpdateAchievementsAsync()
        {
            foreach (var achievement in _currentProgress.Achievements)
            {
                switch (achievement.Id)
                {
                    case "STREAK_7":
                        if (_currentProgress.CurrentStreak >= 7 && !achievement.IsUnlocked)
                        {
                            await UnlockAchievement(achievement);
                        }
                        achievement.ProgressCurrent = _currentProgress.CurrentStreak;
                        break;

                    case "SIGNS_50":
                        if (_currentProgress.SignsLearned >= 50 && !achievement.IsUnlocked)
                        {
                            await UnlockAchievement(achievement);
                        }
                        achievement.ProgressCurrent = _currentProgress.SignsLearned;
                        break;

                    case "QUIZ_PERFECT":
                        var perfectQuiz = _currentProgress.Activities
                            .Any(a => a.Type == ActivityType.Quiz && a.Score == "10/10");
                        if (perfectQuiz && !achievement.IsUnlocked)
                        {
                            await UnlockAchievement(achievement);
                        }
                        achievement.ProgressCurrent = perfectQuiz ? 1 : 0;
                        break;
                }
            }
        }

        private async Task UnlockAchievement(Achievement achievement)
        {
            achievement.IsUnlocked = true;
            achievement.UnlockedDate = DateTime.Now;

            await LogActivityAsync(new ActivityLog
            {
                Id = Guid.NewGuid().ToString(),
                Type = ActivityType.Achievement,
                Description = $"Achievement Unlocked: {achievement.Title}",
                IconName = achievement.IconName,
                Timestamp = DateTime.Now
            });
        }

        public async Task<bool> UpdateStreakAsync()
        {
            var lastActivity = _currentProgress.Activities
                .OrderByDescending(a => a.Timestamp)
                .FirstOrDefault();

            if (lastActivity == null)
            {
                _currentProgress.CurrentStreak = 1;
                return true;
            }

            var today = DateTime.Today;
            var lastActivityDate = lastActivity.Timestamp.Date;

            if (lastActivityDate == today)
                return false;

            if (lastActivityDate == today.AddDays(-1))
                _currentProgress.CurrentStreak++;
            else
                _currentProgress.CurrentStreak = 1;

            await SaveProgressAsync(_currentProgress);
            await UpdateAchievementsAsync();
            return true;
        }

        public async Task SaveProgressAsync(UserProgress progress)
        {
            var json = JsonSerializer.Serialize(progress);
            await File.WriteAllTextAsync(_progressFile, json);
            _currentProgress = progress;
        }
    }
}
