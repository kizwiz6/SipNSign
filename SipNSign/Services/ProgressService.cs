using com.kizwiz.sipnsign.Models;
using System.Diagnostics;
using System.Text.Json;

namespace com.kizwiz.sipnsign.Services
{
    /// <summary>
    /// Manages user progress, achievements, and activity tracking
    /// </summary>
    public class ProgressService : IProgressService
    {
        #region Fields
        private readonly string _progressFile;
        private UserProgress _currentProgress;
        private readonly List<Achievement> _availableAchievements;
        #endregion

        #region Constructor
        public ProgressService()
        {
            try
            {
                Debug.WriteLine("Initializing ProgressService");
                _progressFile = Path.Combine(FileSystem.AppDataDirectory, "progress.json");
                Debug.WriteLine($"Progress file path: {_progressFile}");

                _availableAchievements = InitializeAchievements();
                Debug.WriteLine("Achievements initialised");

                _currentProgress = LoadProgress().Result;
                Debug.WriteLine("Progress loaded successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR in ProgressService constructor: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Retrieves the current user progress asynchronously.
        /// </summary>
        /// <returns>A Task that represents the asynchronous operation, with a UserProgress result.</returns>
        public async Task<UserProgress> GetUserProgressAsync()
        {
            // Simulate an async operation, even if there's none for now.
            return await Task.FromResult(_currentProgress);
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

        public async Task UpdateAchievementsAsync()
        {
            foreach (var achievement in _currentProgress.Achievements)
            {
                switch (achievement.Id)
                {
                    case "FIRST_SIGN" when _currentProgress.SignsLearned >= 1:
                        await UnlockAchievement(achievement);
                        achievement.ProgressCurrent = 1;
                        break;

                    case "STREAK_7" when _currentProgress.CurrentStreak >= 7:
                        await UnlockAchievement(achievement);
                        achievement.ProgressCurrent = _currentProgress.CurrentStreak;
                        break;

                    case "STREAK_30" when _currentProgress.CurrentStreak >= 30:
                        await UnlockAchievement(achievement);
                        achievement.ProgressCurrent = _currentProgress.CurrentStreak;
                        break;

                    case "SIGNS_50" when !achievement.IsUnlocked:
                        achievement.ProgressCurrent = Math.Min(_currentProgress.SignsLearned, achievement.ProgressRequired);
                        if (!achievement.IsUnlocked && _currentProgress.SignsLearned >= 50)
                        {
                            await UnlockAchievement(achievement);
                        }
                        break;

                    case "SIGNS_100" when !achievement.IsUnlocked:
                        achievement.ProgressCurrent = Math.Min(_currentProgress.SignsLearned, achievement.ProgressRequired);
                        if (!achievement.IsUnlocked && _currentProgress.SignsLearned >= 100)
                        {
                            await UnlockAchievement(achievement);
                        }
                        break;

                    case "QUIZ_PERFECT":
                        var perfectQuiz = _currentProgress.Activities
                            .Any(a => a.Type == ActivityType.Quiz && a.Score == "10/10");
                        if (perfectQuiz)
                        {
                            await UnlockAchievement(achievement);
                        }
                        achievement.ProgressCurrent = perfectQuiz ? 1 : 0;
                        break;

                    case "PRACTICE_HOURS_10" when _currentProgress.TotalPracticeTime.TotalHours >= 10:
                        await UnlockAchievement(achievement);
                        achievement.ProgressCurrent = (int)_currentProgress.TotalPracticeTime.TotalHours;
                        break;

                    case "SIGNS_100_GUESS" when !achievement.IsUnlocked:
                        achievement.ProgressCurrent = Math.Min(_currentProgress.GuessModeSigns, achievement.ProgressRequired);
                        if (!achievement.IsUnlocked && _currentProgress.GuessModeSigns >= 100)
                        {
                            await UnlockAchievement(achievement);
                        }
                        break;

                    case "SIGNS_1000_GUESS" when !achievement.IsUnlocked:
                        achievement.ProgressCurrent = Math.Min(_currentProgress.GuessModeSigns, achievement.ProgressRequired);
                        if (!achievement.IsUnlocked && _currentProgress.GuessModeSigns >= 1000)
                        {
                            await UnlockAchievement(achievement);
                            achievement.ProgressCurrent = 1000;  // Cap at required amount
                        }
                        break;

                    case "SIGNS_100_PERFORM" when !achievement.IsUnlocked:
                        achievement.ProgressCurrent = Math.Min(_currentProgress.PerformModeSigns, achievement.ProgressRequired);
                        if (!achievement.IsUnlocked && _currentProgress.PerformModeSigns >= 100)
                        {
                            await UnlockAchievement(achievement);
                        }
                        break;

                    case "SIGNS_1000_PERFORM" when !achievement.IsUnlocked:
                        achievement.ProgressCurrent = Math.Min(_currentProgress.PerformModeSigns, achievement.ProgressRequired);
                        if (!achievement.IsUnlocked && _currentProgress.PerformModeSigns >= 1000)
                        {
                            await UnlockAchievement(achievement);
                        }
                        break;

                    case "PARTY_STARTER" when !achievement.IsUnlocked:
                        // Count completed Perform mode sessions
                        var performSessions = _currentProgress.Activities
                            .Count(a => a.Type == ActivityType.Practice &&
                                       a.Description.Contains("Perform Mode completed"));
                        achievement.ProgressCurrent = performSessions;
                        if (performSessions >= 50)
                        {
                            await UnlockAchievement(achievement);
                        }
                        break;

                    case "SOCIAL_BUTTERFLY" when !achievement.IsUnlocked:
                        // Group activities by date and check if both modes were played
                        var dailyModes = _currentProgress.Activities
                            .GroupBy(a => a.Timestamp.Date)
                            .Count(g => g.Any(a => a.Description.Contains("Guess Mode")) &&
                                        g.Any(a => a.Description.Contains("Perform Mode")));
                        achievement.ProgressCurrent = dailyModes;
                        if (dailyModes >= 5)
                        {
                            await UnlockAchievement(achievement);
                        }
                        break;

                    case "RAPID_FIRE" when !achievement.IsUnlocked:
                        // Count correct answers under 5 seconds
                        var rapidAnswers = _currentProgress.Activities
                            .Count(a => a.Type == ActivityType.Practice &&
                                       a.Score == "+1" &&
                                       a.Description.Contains("under 5 seconds"));
                        achievement.ProgressCurrent = rapidAnswers;
                        if (rapidAnswers >= 50)
                        {
                            await UnlockAchievement(achievement);
                        }
                        break;

                    case "SPEED_MASTER" when !achievement.IsUnlocked:
                        // Check for completed sessions with average time under 3 seconds
                        var fastSessions = _currentProgress.Activities
                            .Count(a => a.Type == ActivityType.Practice &&
                                       a.Description.Contains("Guess Mode completed") &&
                                       a.Description.Contains("average time under 3 seconds"));
                        achievement.ProgressCurrent = fastSessions;
                        if (fastSessions >= 100)
                        {
                            await UnlockAchievement(achievement);
                        }
                        break;

                    case "PERFECT_SESSION" when _currentProgress.CorrectInARow >= 50:
                        await UnlockAchievement(achievement);
                        achievement.ProgressCurrent = 1;
                        break;
                }
            }
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
            var today = DateTime.Today;
            var lastActivity = progress.Activities
                .OrderByDescending(a => a.Timestamp)
                .FirstOrDefault();

            if (lastActivity != null)
            {
                var lastActivityDate = lastActivity.Timestamp.Date;
                if (lastActivityDate == today)
                {
                    // Already updated today, keep current streak
                }
                else if (lastActivityDate == today.AddDays(-1))
                {
                    progress.CurrentStreak++; // Increment streak for consecutive days
                }
                else
                {
                    progress.CurrentStreak = 1; // Reset streak if missed a day
                }
            }
            else
            {
                progress.CurrentStreak = 1; // First activity
            }

            var json = JsonSerializer.Serialize(progress);
            await File.WriteAllTextAsync(_progressFile, json);
            _currentProgress = progress;
        }
        #endregion

        #region Private Methods
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
                },
                new Achievement
                {
                    Id = "FIRST_SIGN",
                    Title = "First Steps",
                    Description = "Learn your first sign",
                    IconName = "practice_icon",
                    ProgressRequired = 1
                },
                new Achievement
                {
                    Id = "STREAK_30",
                    Title = "Monthly Master",
                    Description = "Practice for 30 consecutive days",
                    IconName = "streak_icon",
                    ProgressRequired = 30
                },
                new Achievement
                {
                    Id = "SIGNS_100",
                    Title = "Century Club",
                    Description = "Learn 100 signs",
                    IconName = "practice_icon",
                    ProgressRequired = 100
                },
                new Achievement
                {
                    Id = "PRACTICE_HOURS_10",
                    Title = "Dedicated Student",
                    Description = "Practice for 10 hours",
                    IconName = "time_icon",
                    ProgressRequired = 10
                },
                new Achievement
                {
                    Id = "SIGNS_100_GUESS",
                    Title = "Guess Master",
                    Description = "Get 100 signs correct in Guess Mode",
                    IconName = "mastery_icon",
                    ProgressRequired = 100
                },
                new Achievement
                {
                    Id = "SIGNS_1000_GUESS",
                    Title = "Ultimate Guesser",
                    Description = "Get 1000 signs correct in Guess Mode",
                    IconName = "mastery_icon",
                    ProgressRequired = 1000
                },
                new Achievement
                {
                    Id = "SIGNS_100_PERFORM",
                    Title = "Performance Pro",
                    Description = "Successfully perform 100 signs",
                    IconName = "mastery_icon",
                    ProgressRequired = 100
                },
                new Achievement
                {
                    Id = "SIGNS_1000_PERFORM",
                    Title = "Sign Language Star",
                    Description = "Successfully perform 1000 signs",
                    IconName = "mastery_icon",
                    ProgressRequired = 1000
                },
                new Achievement
                {
                    Id = "PERFECT_SESSION",
                    Title = "Perfect Session",
                    Description = "Get 50 signs correct in a row",
                    IconName = "quiz_icon",
                    ProgressRequired = 1
                },
                new Achievement {
                    Id = "PARTY_STARTER",
                    Title = "Party Starter",
                    Description = "Played 50 complete sessions in Perform mode",
                    IconName = "party_icon",
                    ProgressRequired = 50
                },
                new Achievement {
                    Id = "SOCIAL_BUTTERFLY",
                    Title = "Social Butterfly",
                    Description = "Play in both modes in the same day for 5 days",
                    IconName = "social_icon",
                    ProgressRequired = 5
                },
                new Achievement {
                    Id = "RAPID_FIRE",
                    Title = "Rapid Fire",
                    Description = "Answer 50 signs correctly in under 5 seconds each",
                    IconName = "speed_icon",
                    ProgressRequired = 5
                },
                new Achievement {
                    Id = "SPEED_MASTER",
                    Title = "Speed Master",
                    Description = "Complete a 100 Guess Mode session with average time under 3 seconds",
                    IconName = "lightning_icon",
                    ProgressRequired = 100
                }
            };
        }

        private Task<UserProgress> LoadProgress()
        {
            try
            {
                Debug.WriteLine("Starting LoadProgress");

                // Check for existing progress file
                if (File.Exists(_progressFile))
                {
                    Debug.WriteLine("Found existing progress file");
                    try
                    {
                        var json = File.ReadAllText(_progressFile);
                        Debug.WriteLine("Read existing progress file");

                        var options = new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                        };

                        var progress = JsonSerializer.Deserialize<UserProgress>(json, options);
                        if (progress != null)
                        {
                            Debug.WriteLine("Successfully loaded existing progress");
                            return Task.FromResult(progress);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error loading existing progress: {ex.Message}");
                        // If there's an error reading the file, we'll create a new one
                    }
                }

                Debug.WriteLine("Creating new progress");
                var newProgress = new UserProgress
                {
                    SignsLearned = 0,
                    CurrentStreak = 0,
                    Accuracy = 0,
                    TotalPracticeTime = TimeSpan.Zero,
                    TotalAttempts = 0,
                    CorrectAttempts = 0,
                    GuessModeSigns = 0,
                    PerformModeSigns = 0,
                    Achievements = new List<Achievement>(_availableAchievements),
                    Activities = new List<ActivityLog>()
                };

                try
                {
                    Debug.WriteLine("Saving new progress");
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                    };
                    var json = JsonSerializer.Serialize(newProgress, options);
                    File.WriteAllText(_progressFile, json);
                    Debug.WriteLine("New progress saved successfully");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error saving new progress: {ex.Message}");
                    // Even if save fails, return the new progress object
                }

                Debug.WriteLine("Returning progress object");
                return Task.FromResult(newProgress);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CRITICAL ERROR in LoadProgress: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private UserProgress InitializeNewProgress()
        {
            return new UserProgress
            {
                SignsLearned = 0,
                CurrentStreak = 0,
                Accuracy = 0,
                TotalPracticeTime = TimeSpan.Zero,
                Achievements = _availableAchievements,
                Activities = new List<ActivityLog>(),
                TotalAttempts = 0,
                CorrectAttempts = 0,
                GuessModeSigns = 0,
                PerformModeSigns = 0
            };
        }

        private void UpdateStatistics(ActivityLog activity)
        {
            switch (activity.Type)
            {
                case ActivityType.Practice:
                    if (activity.Score == "+1")
                    {
                        // Update accuracy
                        int totalAttempts = _currentProgress.Activities.Count(a => a.Type == ActivityType.Practice);
                        int correctAttempts = _currentProgress.Activities.Count(a => a.Type == ActivityType.Practice && a.Score == "+1");
                        _currentProgress.Accuracy = (double)correctAttempts / totalAttempts;

                        // Update practice time (increment by 30 seconds per attempt)
                        _currentProgress.TotalPracticeTime = _currentProgress.TotalPracticeTime.Add(TimeSpan.FromSeconds(30));
                    }
                    break;
            }

            // Update streak
            var today = DateTime.Today;
            var lastActivity = _currentProgress.Activities
                .OrderByDescending(a => a.Timestamp)
                .FirstOrDefault();

            if (lastActivity != null)
            {
                var lastActivityDate = lastActivity.Timestamp.Date;
                if (lastActivityDate == today.AddDays(-1))
                {
                    _currentProgress.CurrentStreak++;
                }
                else if (lastActivityDate != today)
                {
                    _currentProgress.CurrentStreak = 1;
                }
            }
            else
            {
                _currentProgress.CurrentStreak = 1;
            }
        }

        private async Task UnlockAchievement(Achievement achievement)
        {
            // Only unlock and log if it hasn't been unlocked before
            if (!achievement.IsUnlocked)
            {
                achievement.IsUnlocked = true;
                achievement.UnlockedDate = DateTime.Now;

                // Use the appropriate icon based on achievement type
                string iconName = achievement.Id switch
                {
                    "FIRST_SIGN" => "first_sign_icon",
                    "STREAK_7" => "streak_icon",
                    "STREAK_30" => "streak_icon",
                    "SIGNS_50" => "mastery_icon",
                    "SIGNS_100" => "mastery_icon",
                    "QUIZ_PERFECT" => "quiz_icon",
                    "PRACTICE_HOURS_10" => "time_icon",
                    "PARTY_STARTER" => "party_icon",
                    "SOCIAL_BUTTERFLY" => "social_icon",
                    "RAPID_FIRE" => "speed_icon",
                    "SPEED_MASTER" => "lightning_icon",
                    _ => "achievement_icon"  // Default case (underscore, not asterisk)
                };

                await LogActivityAsync(new ActivityLog
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = ActivityType.Achievement,
                    Description = $"Achievement Unlocked: {achievement.Title}",
                    IconName = iconName,
                    Timestamp = DateTime.Now
                });
            }
        }
        #endregion
    }
}
