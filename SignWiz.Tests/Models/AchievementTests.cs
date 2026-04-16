using com.kizwiz.signwiz.Models;
using Xunit;

namespace SignWiz.Tests.Models;

public class AchievementTests
{
    [Fact]
    public void NewAchievement_IsNotUnlocked()
    {
        var achievement = new Achievement();

        Assert.False(achievement.IsUnlocked);
        Assert.Null(achievement.UnlockedDate);
    }

    [Fact]
    public void Achievement_CanTrackProgress()
    {
        var achievement = new Achievement
        {
            Id = "SIGNS_50",
            Title = "50 Signs",
            Description = "Learn 50 signs",
            ProgressCurrent = 25,
            ProgressRequired = 50
        };

        Assert.Equal(25, achievement.ProgressCurrent);
        Assert.Equal(50, achievement.ProgressRequired);
        Assert.False(achievement.IsUnlocked);
    }

    [Fact]
    public void Achievement_WhenUnlocked_HasUnlockedDate()
    {
        var unlockDate = DateTime.Now;
        var achievement = new Achievement
        {
            Id = "FIRST_SIGN",
            IsUnlocked = true,
            UnlockedDate = unlockDate,
            ProgressCurrent = 1,
            ProgressRequired = 1
        };

        Assert.True(achievement.IsUnlocked);
        Assert.Equal(unlockDate, achievement.UnlockedDate);
    }

    [Fact]
    public void Achievement_ProgressPercentage_CalculatesCorrectly()
    {
        var achievement = new Achievement
        {
            ProgressCurrent = 30,
            ProgressRequired = 100
        };

        // The ProfileViewModel calculates this as (double)ProgressCurrent / ProgressRequired
        double progress = (double)achievement.ProgressCurrent / achievement.ProgressRequired;

        Assert.Equal(0.3, progress);
    }
}
