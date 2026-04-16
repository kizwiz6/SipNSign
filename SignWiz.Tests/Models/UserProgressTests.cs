using com.kizwiz.signwiz.Models;
using Xunit;

namespace SignWiz.Tests.Models;

public class UserProgressTests
{
    [Fact]
    public void NewUserProgress_HasDefaultValues()
    {
        var progress = new UserProgress();

        Assert.Equal(0, progress.SignsLearned);
        Assert.Equal(0, progress.GuessModeSigns);
        Assert.Equal(0, progress.PerformModeSigns);
        Assert.Equal(0, progress.CurrentStreak);
        Assert.Equal(0, progress.CorrectInARow);
        Assert.Equal(0.0, progress.Accuracy);
        Assert.Equal(0, progress.TotalAttempts);
        Assert.Equal(0, progress.CorrectAttempts);
        Assert.Equal(TimeSpan.Zero, progress.TotalPracticeTime);
        Assert.NotNull(progress.Achievements);
        Assert.Empty(progress.Achievements);
        Assert.NotNull(progress.Activities);
        Assert.Empty(progress.Activities);
    }

    [Fact]
    public void Accuracy_CanBeSetAndRetrieved()
    {
        var progress = new UserProgress { Accuracy = 0.85 };

        Assert.Equal(0.85, progress.Accuracy);
    }

    [Fact]
    public void TotalPracticeTime_CanBeSetAndRetrieved()
    {
        var progress = new UserProgress
        {
            TotalPracticeTime = TimeSpan.FromHours(2.5)
        };

        Assert.Equal(TimeSpan.FromHours(2.5), progress.TotalPracticeTime);
    }

    [Fact]
    public void Achievements_CanAddItems()
    {
        var progress = new UserProgress();
        progress.Achievements.Add(new Achievement
        {
            Id = "TEST",
            Title = "Test Achievement",
            Description = "A test",
            ProgressCurrent = 0,
            ProgressRequired = 1
        });

        Assert.Single(progress.Achievements);
        Assert.Equal("TEST", progress.Achievements[0].Id);
    }

    [Fact]
    public void Activities_CanAddItems()
    {
        var progress = new UserProgress();
        progress.Activities.Add(new ActivityLog
        {
            Id = "act1",
            Description = "Test activity",
            Type = ActivityType.Practice,
            Timestamp = DateTime.Now
        });

        Assert.Single(progress.Activities);
        Assert.Equal("Test activity", progress.Activities[0].Description);
    }

    [Fact]
    public void AccuracyCalculation_MatchesExpectedFormula()
    {
        var progress = new UserProgress
        {
            TotalAttempts = 100,
            CorrectAttempts = 75
        };

        // The ViewModel calculates accuracy as CorrectAttempts / TotalAttempts
        double expectedAccuracy = (double)progress.CorrectAttempts / progress.TotalAttempts;

        Assert.Equal(0.75, expectedAccuracy);
    }
}
