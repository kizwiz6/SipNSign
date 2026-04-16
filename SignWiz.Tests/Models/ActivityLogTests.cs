using com.kizwiz.signwiz.Models;
using Xunit;

namespace SignWiz.Tests.Models;

public class ActivityLogTests
{
    [Fact]
    public void NewActivityLog_HasDefaultValues()
    {
        var log = new ActivityLog();

        Assert.Null(log.Id);
        Assert.Null(log.Description);
        Assert.Null(log.IconName);
        Assert.Null(log.Score);
        Assert.Null(log.SignName);
        Assert.Equal(default(DateTime), log.Timestamp);
        Assert.Equal(default(ActivityType), log.Type);
    }

    [Fact]
    public void ActivityLog_PracticeType_CanBeCreated()
    {
        var log = new ActivityLog
        {
            Id = Guid.NewGuid().ToString(),
            Type = ActivityType.Practice,
            Description = "Correctly signed 'Hello'",
            IconName = "quiz_correct_icon",
            Timestamp = DateTime.Now,
            Score = "+1",
            SignName = "Hello"
        };

        Assert.Equal(ActivityType.Practice, log.Type);
        Assert.Equal("+1", log.Score);
        Assert.Equal("Hello", log.SignName);
    }

    [Fact]
    public void ActivityLog_AchievementType_CanBeCreated()
    {
        var log = new ActivityLog
        {
            Id = Guid.NewGuid().ToString(),
            Type = ActivityType.Achievement,
            Description = "Shared an achievement",
            IconName = "achievement_icon",
            Timestamp = DateTime.Now
        };

        Assert.Equal(ActivityType.Achievement, log.Type);
    }

    [Fact]
    public void ActivityLog_QuizType_WithScore()
    {
        var log = new ActivityLog
        {
            Id = Guid.NewGuid().ToString(),
            Type = ActivityType.Quiz,
            Description = "Completed quiz",
            Score = "8/10",
            Timestamp = DateTime.Now
        };

        Assert.Equal(ActivityType.Quiz, log.Type);
        Assert.Equal("8/10", log.Score);
    }
}
