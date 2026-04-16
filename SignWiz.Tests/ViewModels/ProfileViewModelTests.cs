using com.kizwiz.signwiz.Models;
using com.kizwiz.signwiz.Services;
using com.kizwiz.signwiz.ViewModels;
using Moq;
using Xunit;

namespace SignWiz.Tests.ViewModels;

public class ProfileViewModelTests
{
    private readonly Mock<IProgressService> _progressServiceMock;
    private readonly ProfileViewModel _viewModel;

    public ProfileViewModelTests()
    {
        _progressServiceMock = new Mock<IProgressService>();
        _viewModel = new ProfileViewModel(_progressServiceMock.Object);
    }

    [Fact]
    public void Constructor_WithNullProgressService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ProfileViewModel(null!));
    }

    [Fact]
    public async Task LoadProgressAsync_UpdatesSignsLearned()
    {
        // Arrange
        var progress = new UserProgress
        {
            SignsLearned = 42,
            CurrentStreak = 5,
            Accuracy = 0.85,
            TotalPracticeTime = TimeSpan.FromMinutes(90),
            GuessModeSigns = 30,
            PerformModeSigns = 12,
            Achievements = new List<Achievement>(),
            Activities = new List<ActivityLog>()
        };
        _progressServiceMock
            .Setup(s => s.GetUserProgressAsync())
            .ReturnsAsync(progress);

        // Act
        await _viewModel.LoadProgressAsync();

        // Assert
        Assert.Equal(42, _viewModel.SignsLearned);
    }

    [Fact]
    public async Task LoadProgressAsync_UpdatesCurrentStreak()
    {
        // Arrange
        var progress = new UserProgress
        {
            CurrentStreak = 7,
            Achievements = new List<Achievement>(),
            Activities = new List<ActivityLog>()
        };
        _progressServiceMock
            .Setup(s => s.GetUserProgressAsync())
            .ReturnsAsync(progress);

        // Act
        await _viewModel.LoadProgressAsync();

        // Assert
        Assert.Equal(7, _viewModel.CurrentStreak);
    }

    [Fact]
    public async Task LoadProgressAsync_UpdatesAccuracy()
    {
        // Arrange
        var progress = new UserProgress
        {
            Accuracy = 0.95,
            Achievements = new List<Achievement>(),
            Activities = new List<ActivityLog>()
        };
        _progressServiceMock
            .Setup(s => s.GetUserProgressAsync())
            .ReturnsAsync(progress);

        // Act
        await _viewModel.LoadProgressAsync();

        // Assert
        Assert.Equal(0.95, _viewModel.Accuracy);
        Assert.Equal("95%", _viewModel.AccuracyDisplay);
    }

    [Fact]
    public async Task LoadProgressAsync_PracticeTimeUnderOneHour_DisplaysMinutes()
    {
        // Arrange
        var progress = new UserProgress
        {
            TotalPracticeTime = TimeSpan.FromMinutes(45),
            Achievements = new List<Achievement>(),
            Activities = new List<ActivityLog>()
        };
        _progressServiceMock
            .Setup(s => s.GetUserProgressAsync())
            .ReturnsAsync(progress);

        // Act
        await _viewModel.LoadProgressAsync();

        // Assert
        Assert.Equal("45 min", _viewModel.PracticeTimeDisplay);
    }

    [Fact]
    public async Task LoadProgressAsync_PracticeTimeOverOneHour_DisplaysHours()
    {
        // Arrange
        var progress = new UserProgress
        {
            TotalPracticeTime = TimeSpan.FromHours(2.5),
            Achievements = new List<Achievement>(),
            Activities = new List<ActivityLog>()
        };
        _progressServiceMock
            .Setup(s => s.GetUserProgressAsync())
            .ReturnsAsync(progress);

        // Act
        await _viewModel.LoadProgressAsync();

        // Assert
        Assert.Equal("2.5 hrs", _viewModel.PracticeTimeDisplay);
    }

    [Fact]
    public async Task LoadProgressAsync_PopulatesRecentActivities()
    {
        // Arrange
        var progress = new UserProgress
        {
            Achievements = new List<Achievement>(),
            Activities = new List<ActivityLog>
            {
                new ActivityLog
                {
                    Description = "Practiced Hello",
                    Timestamp = DateTime.Now.AddMinutes(-5),
                    Type = ActivityType.Practice,
                    Score = "+1",
                    SignName = "Hello"
                },
                new ActivityLog
                {
                    Description = "Completed quiz",
                    Timestamp = DateTime.Now.AddMinutes(-10),
                    Type = ActivityType.Quiz,
                    Score = "8/10"
                }
            }
        };
        _progressServiceMock
            .Setup(s => s.GetUserProgressAsync())
            .ReturnsAsync(progress);

        // Act
        await _viewModel.LoadProgressAsync();

        // Assert
        Assert.Equal(2, _viewModel.RecentActivities.Count);
    }

    [Fact]
    public async Task LoadProgressAsync_FiltersActivitiesWithEmptyDescription()
    {
        // Arrange
        var progress = new UserProgress
        {
            Achievements = new List<Achievement>(),
            Activities = new List<ActivityLog>
            {
                new ActivityLog
                {
                    Description = "Practiced Hello",
                    Timestamp = DateTime.Now,
                    Type = ActivityType.Practice,
                    Score = "+1"
                },
                new ActivityLog
                {
                    Description = "",
                    Timestamp = DateTime.Now,
                    Type = ActivityType.Practice,
                    Score = "+1"
                }
            }
        };
        _progressServiceMock
            .Setup(s => s.GetUserProgressAsync())
            .ReturnsAsync(progress);

        // Act
        await _viewModel.LoadProgressAsync();

        // Assert
        Assert.Single(_viewModel.RecentActivities);
    }

    [Fact]
    public async Task LoadProgressAsync_LimitsRecentActivitiesToTen()
    {
        // Arrange
        var activities = Enumerable.Range(1, 15).Select(i => new ActivityLog
        {
            Description = $"Activity {i}",
            Timestamp = DateTime.Now.AddMinutes(-i),
            Type = ActivityType.Practice,
            Score = "+1"
        }).ToList();

        var progress = new UserProgress
        {
            Achievements = new List<Achievement>(),
            Activities = activities
        };
        _progressServiceMock
            .Setup(s => s.GetUserProgressAsync())
            .ReturnsAsync(progress);

        // Act
        await _viewModel.LoadProgressAsync();

        // Assert
        Assert.Equal(10, _viewModel.RecentActivities.Count);
    }

    [Fact]
    public async Task LoadProgressAsync_PopulatesAchievements()
    {
        // Arrange
        var progress = new UserProgress
        {
            Activities = new List<ActivityLog>(),
            Achievements = new List<Achievement>
            {
                new Achievement
                {
                    Id = "FIRST_SIGN",
                    Title = "First Sign",
                    Description = "Learn your first sign",
                    IsUnlocked = true,
                    ProgressCurrent = 1,
                    ProgressRequired = 1
                },
                new Achievement
                {
                    Id = "SIGNS_50",
                    Title = "50 Signs",
                    Description = "Learn 50 signs",
                    IsUnlocked = false,
                    ProgressCurrent = 10,
                    ProgressRequired = 50
                }
            }
        };
        _progressServiceMock
            .Setup(s => s.GetUserProgressAsync())
            .ReturnsAsync(progress);

        // Act
        await _viewModel.LoadProgressAsync();

        // Assert
        Assert.Equal(2, _viewModel.Achievements.Count);
    }

    [Fact]
    public async Task LoadProgressAsync_AchievementsHeaderText_ShowsCorrectCounts()
    {
        // Arrange
        var progress = new UserProgress
        {
            Activities = new List<ActivityLog>(),
            Achievements = new List<Achievement>
            {
                new Achievement { Id = "A1", Title = "A1", IsUnlocked = true, ProgressCurrent = 1, ProgressRequired = 1 },
                new Achievement { Id = "A2", Title = "A2", IsUnlocked = false, ProgressCurrent = 0, ProgressRequired = 5 },
                new Achievement { Id = "A3", Title = "A3", IsUnlocked = true, ProgressCurrent = 3, ProgressRequired = 3 }
            }
        };
        _progressServiceMock
            .Setup(s => s.GetUserProgressAsync())
            .ReturnsAsync(progress);

        // Act
        await _viewModel.LoadProgressAsync();

        // Assert
        Assert.Equal("Achievements (2/3)", _viewModel.AchievementsHeaderText);
    }

    [Fact]
    public void PropertyChanged_RaisedWhenSignsLearnedChanges()
    {
        // Arrange
        var raised = false;
        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ProfileViewModel.SignsLearned))
                raised = true;
        };

        // Act
        _viewModel.SignsLearned = 10;

        // Assert
        Assert.True(raised);
    }
}
