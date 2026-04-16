using com.kizwiz.signwiz.Models;
using Xunit;

namespace SignWiz.Tests.Models;

public class AchievementTests
{
    #region Helper Methods

    /// <summary>
    /// Creates an achievement matching the definitions in ProgressService.InitializeAchievements.
    /// </summary>
    private static Achievement CreateAchievement(string id, string title, string description, string iconName, int progressRequired)
    {
        return new Achievement
        {
            Id = id,
            Title = title,
            Description = description,
            IconName = iconName,
            ProgressRequired = progressRequired
        };
    }

    /// <summary>
    /// Simulates unlocking an achievement (mirrors ProgressService.UnlockAchievement).
    /// </summary>
    private static void SimulateUnlock(Achievement achievement, int progressCurrent)
    {
        if (!achievement.IsUnlocked)
        {
            achievement.IsUnlocked = true;
            achievement.UnlockedDate = DateTime.Now;
        }
        achievement.ProgressCurrent = progressCurrent;
    }

    /// <summary>
    /// Returns all achievements as defined in ProgressService.InitializeAchievements.
    /// </summary>
    private static List<Achievement> GetAllAchievements()
    {
        return new List<Achievement>
        {
            CreateAchievement("STREAK_7", "Week Warrior", "Practiced for 7 consecutive days", "streak_weekly_icon", 7),
            CreateAchievement("SIGNS_50", "Sign Master", "Learnt 50 signs", "fifty_signs_icon", 50),
            CreateAchievement("SIGNS_100", "Century Club", "Learnt 100 signs", "century_club_icon", 100),
            CreateAchievement("SIGNS_100_GUESS", "Guess Master", "Got 100 signs correct in Guess Mode", "guess_100_icon", 100),
            CreateAchievement("SIGNS_1000_GUESS", "Ultimate Guesser", "Got 1000 signs correct in Guess Mode", "guess_1000_icon", 1000),
            CreateAchievement("SIGNS_100_PERFORM", "Performance Pro", "Successfully performed 100 signs", "perform_100_icon", 100),
            CreateAchievement("SIGNS_1000_PERFORM", "Sign Language Star", "Successfully performed 1000 signs", "perform_1000_icon", 1000),
            CreateAchievement("QUIZ_PERFECT", "Perfect Score", "Got 100% on a 100-question quiz", "quiz_master_icon", 1),
            CreateAchievement("FIRST_SIGN", "First Steps", "Learnt your first sign", "first_sign_icon", 1),
            CreateAchievement("STREAK_30", "Monthly Master", "Practiced for 30 consecutive days", "streak_monthly_icon", 30),
            CreateAchievement("PRACTICE_HOURS_10", "Dedicated Student", "Practiced for 10 hours", "time_icon", 10),
            CreateAchievement("PERFECT_SESSION", "Perfect Session", "Got 50 signs correct in a row", "perfect_session_icon", 1),
            CreateAchievement("SOCIAL_BUTTERFLY", "Social Butterfly", "Shared your first achievement", "social_icon", 1),
            CreateAchievement("RAPID_FIRE", "Rapid Fire", "Answered 50 signs correctly in under 5 seconds each", "speed_icon", 5),
            CreateAchievement("SPEED_MASTER", "Speed Master", "Completed a 100 Guess Mode session with average time under 3 seconds", "speed_master_icon", 1),
            CreateAchievement("PARTY_HOST", "Party Host", "Complete a multiplayer game with 5+ players", "party_icon", 1),
            CreateAchievement("MULTIPLAYER_FIRST", "Social Learner", "Complete your first multiplayer game", "multiplayer_icon", 1),
            CreateAchievement("MULTIPLAYER_10", "Party Regular", "Complete 10 multiplayer games", "multiplayer_10_icon", 10),
            CreateAchievement("MULTIPLAYER_50", "Social Butterfly Pro", "Complete 50 multiplayer games", "multiplayer_50_icon", 50),
            CreateAchievement("CHAMPION_WIN", "Undefeated", "Win 3 multiplayer games in a row", "champion_icon", 3),
            CreateAchievement("CLOSE_CALL", "Nail Biter", "Win a multiplayer game by exactly 1 point", "close_call_icon", 1),
            CreateAchievement("PERFECT_ROUND_ALL", "Team Harmony", "All players get the same sign correct", "harmony_icon", 1),
            CreateAchievement("COMEBACK_KING", "Comeback King", "Win a multiplayer game after being in last place", "comeback_icon", 1),
            CreateAchievement("PERFECT_MULTIPLAYER", "Multiplayer Master", "Get all signs correct in a multiplayer game", "perfect_multi_icon", 1),
            CreateAchievement("PARTY_ANIMAL", "Party Animal", "Play multiplayer games with 10 different group sizes", "party_animal_icon", 10),
        };
    }

    #endregion

    #region General Achievement Tests

    [Fact]
    public void NewAchievement_IsNotUnlocked()
    {
        var achievement = new Achievement();

        Assert.False(achievement.IsUnlocked);
        Assert.Null(achievement.UnlockedDate);
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

        double progress = (double)achievement.ProgressCurrent / achievement.ProgressRequired;

        Assert.Equal(0.3, progress);
    }

    [Fact]
    public void AllAchievements_StartLocked()
    {
        var achievements = GetAllAchievements();

        Assert.All(achievements, a =>
        {
            Assert.False(a.IsUnlocked);
            Assert.Null(a.UnlockedDate);
            Assert.Equal(0, a.ProgressCurrent);
        });
    }

    [Fact]
    public void AllAchievements_HaveUniqueIds()
    {
        var achievements = GetAllAchievements();
        var ids = achievements.Select(a => a.Id).ToList();

        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void AllAchievements_HaveRequiredProperties()
    {
        var achievements = GetAllAchievements();

        Assert.All(achievements, a =>
        {
            Assert.False(string.IsNullOrWhiteSpace(a.Id));
            Assert.False(string.IsNullOrWhiteSpace(a.Title));
            Assert.False(string.IsNullOrWhiteSpace(a.Description));
            Assert.False(string.IsNullOrWhiteSpace(a.IconName));
            Assert.True(a.ProgressRequired > 0);
        });
    }

    [Fact]
    public void AllAchievements_TotalCount_Is25()
    {
        var achievements = GetAllAchievements();
        Assert.Equal(25, achievements.Count);
    }

    #endregion

    #region First Sign Achievement

    [Fact]
    public void FirstSign_Unlocks_WhenSignsLearnedIsAtLeast1()
    {
        var achievement = CreateAchievement("FIRST_SIGN", "First Steps", "Learnt your first sign", "first_sign_icon", 1);
        int signsLearned = 1;

        if (signsLearned >= 1)
            SimulateUnlock(achievement, 1);

        Assert.True(achievement.IsUnlocked);
        Assert.NotNull(achievement.UnlockedDate);
        Assert.Equal(1, achievement.ProgressCurrent);
    }

    [Fact]
    public void FirstSign_DoesNotUnlock_WhenNoSignsLearned()
    {
        var achievement = CreateAchievement("FIRST_SIGN", "First Steps", "Learnt your first sign", "first_sign_icon", 1);
        int signsLearned = 0;

        if (signsLearned >= 1)
            SimulateUnlock(achievement, 1);

        Assert.False(achievement.IsUnlocked);
    }

    #endregion

    #region Streak Achievements

    [Fact]
    public void Streak7_Unlocks_WhenStreakIsAtLeast7()
    {
        var achievement = CreateAchievement("STREAK_7", "Week Warrior", "Practiced for 7 consecutive days", "streak_weekly_icon", 7);
        int currentStreak = 7;

        if (currentStreak >= 7)
            SimulateUnlock(achievement, currentStreak);

        Assert.True(achievement.IsUnlocked);
        Assert.Equal(7, achievement.ProgressCurrent);
    }

    [Fact]
    public void Streak7_DoesNotUnlock_WhenStreakIsBelow7()
    {
        var achievement = CreateAchievement("STREAK_7", "Week Warrior", "Practiced for 7 consecutive days", "streak_weekly_icon", 7);
        int currentStreak = 5;

        if (currentStreak >= 7)
            SimulateUnlock(achievement, currentStreak);

        Assert.False(achievement.IsUnlocked);
    }

    [Fact]
    public void Streak30_Unlocks_WhenStreakIsAtLeast30()
    {
        var achievement = CreateAchievement("STREAK_30", "Monthly Master", "Practiced for 30 consecutive days", "streak_monthly_icon", 30);
        int currentStreak = 30;

        if (currentStreak >= 30)
            SimulateUnlock(achievement, currentStreak);

        Assert.True(achievement.IsUnlocked);
        Assert.Equal(30, achievement.ProgressCurrent);
    }

    [Fact]
    public void Streak30_DoesNotUnlock_WhenStreakIsBelow30()
    {
        var achievement = CreateAchievement("STREAK_30", "Monthly Master", "Practiced for 30 consecutive days", "streak_monthly_icon", 30);
        int currentStreak = 20;

        if (currentStreak >= 30)
            SimulateUnlock(achievement, currentStreak);

        Assert.False(achievement.IsUnlocked);
    }

    #endregion

    #region Signs Learned Achievements

    [Fact]
    public void Signs50_Unlocks_WhenSignsLearnedIsAtLeast50()
    {
        var achievement = CreateAchievement("SIGNS_50", "Sign Master", "Learnt 50 signs", "fifty_signs_icon", 50);
        int signsLearned = 50;

        achievement.ProgressCurrent = Math.Min(signsLearned, achievement.ProgressRequired);
        if (signsLearned >= 50)
            SimulateUnlock(achievement, achievement.ProgressCurrent);

        Assert.True(achievement.IsUnlocked);
        Assert.Equal(50, achievement.ProgressCurrent);
    }

    [Fact]
    public void Signs50_TracksProgress_WhenBelow50()
    {
        var achievement = CreateAchievement("SIGNS_50", "Sign Master", "Learnt 50 signs", "fifty_signs_icon", 50);
        int signsLearned = 25;

        achievement.ProgressCurrent = Math.Min(signsLearned, achievement.ProgressRequired);

        Assert.False(achievement.IsUnlocked);
        Assert.Equal(25, achievement.ProgressCurrent);
    }

    [Fact]
    public void Signs100_Unlocks_WhenSignsLearnedIsAtLeast100()
    {
        var achievement = CreateAchievement("SIGNS_100", "Century Club", "Learnt 100 signs", "century_club_icon", 100);
        int signsLearned = 100;

        achievement.ProgressCurrent = Math.Min(signsLearned, achievement.ProgressRequired);
        if (signsLearned >= 100)
            SimulateUnlock(achievement, achievement.ProgressCurrent);

        Assert.True(achievement.IsUnlocked);
        Assert.Equal(100, achievement.ProgressCurrent);
    }

    [Fact]
    public void Signs100_TracksProgress_WhenBelow100()
    {
        var achievement = CreateAchievement("SIGNS_100", "Century Club", "Learnt 100 signs", "century_club_icon", 100);
        int signsLearned = 75;

        achievement.ProgressCurrent = Math.Min(signsLearned, achievement.ProgressRequired);

        Assert.False(achievement.IsUnlocked);
        Assert.Equal(75, achievement.ProgressCurrent);
    }

    #endregion

    #region Guess Mode Achievements

    [Fact]
    public void Signs100Guess_Unlocks_WhenGuessModeSigns100()
    {
        var achievement = CreateAchievement("SIGNS_100_GUESS", "Guess Master", "Got 100 signs correct in Guess Mode", "guess_100_icon", 100);
        int guessModeSigns = 100;

        achievement.ProgressCurrent = Math.Min(guessModeSigns, achievement.ProgressRequired);
        if (guessModeSigns >= 100)
            SimulateUnlock(achievement, achievement.ProgressCurrent);

        Assert.True(achievement.IsUnlocked);
        Assert.Equal(100, achievement.ProgressCurrent);
    }

    [Fact]
    public void Signs100Guess_TracksProgress_WhenBelow100()
    {
        var achievement = CreateAchievement("SIGNS_100_GUESS", "Guess Master", "Got 100 signs correct in Guess Mode", "guess_100_icon", 100);
        int guessModeSigns = 60;

        achievement.ProgressCurrent = Math.Min(guessModeSigns, achievement.ProgressRequired);

        Assert.False(achievement.IsUnlocked);
        Assert.Equal(60, achievement.ProgressCurrent);
    }

    [Fact]
    public void Signs1000Guess_Unlocks_WhenGuessModeSigns1000()
    {
        var achievement = CreateAchievement("SIGNS_1000_GUESS", "Ultimate Guesser", "Got 1000 signs correct in Guess Mode", "guess_1000_icon", 1000);
        int guessModeSigns = 1000;

        achievement.ProgressCurrent = Math.Min(guessModeSigns, achievement.ProgressRequired);
        if (guessModeSigns >= 1000)
            SimulateUnlock(achievement, 1000);

        Assert.True(achievement.IsUnlocked);
        Assert.Equal(1000, achievement.ProgressCurrent);
    }

    [Fact]
    public void Signs1000Guess_TracksProgress_WhenBelow1000()
    {
        var achievement = CreateAchievement("SIGNS_1000_GUESS", "Ultimate Guesser", "Got 1000 signs correct in Guess Mode", "guess_1000_icon", 1000);
        int guessModeSigns = 500;

        achievement.ProgressCurrent = Math.Min(guessModeSigns, achievement.ProgressRequired);

        Assert.False(achievement.IsUnlocked);
        Assert.Equal(500, achievement.ProgressCurrent);
    }

    #endregion

    #region Perform Mode Achievements

    [Fact]
    public void Signs100Perform_Unlocks_WhenPerformModeSigns100()
    {
        var achievement = CreateAchievement("SIGNS_100_PERFORM", "Performance Pro", "Successfully performed 100 signs", "perform_100_icon", 100);
        int performModeSigns = 100;

        achievement.ProgressCurrent = Math.Min(performModeSigns, achievement.ProgressRequired);
        if (performModeSigns >= 100)
            SimulateUnlock(achievement, achievement.ProgressCurrent);

        Assert.True(achievement.IsUnlocked);
        Assert.Equal(100, achievement.ProgressCurrent);
    }

    [Fact]
    public void Signs100Perform_TracksProgress_WhenBelow100()
    {
        var achievement = CreateAchievement("SIGNS_100_PERFORM", "Performance Pro", "Successfully performed 100 signs", "perform_100_icon", 100);
        int performModeSigns = 40;

        achievement.ProgressCurrent = Math.Min(performModeSigns, achievement.ProgressRequired);

        Assert.False(achievement.IsUnlocked);
        Assert.Equal(40, achievement.ProgressCurrent);
    }

    [Fact]
    public void Signs1000Perform_Unlocks_WhenPerformModeSigns1000()
    {
        var achievement = CreateAchievement("SIGNS_1000_PERFORM", "Sign Language Star", "Successfully performed 1000 signs", "perform_1000_icon", 1000);
        int performModeSigns = 1000;

        achievement.ProgressCurrent = Math.Min(performModeSigns, achievement.ProgressRequired);
        if (performModeSigns >= 1000)
            SimulateUnlock(achievement, achievement.ProgressCurrent);

        Assert.True(achievement.IsUnlocked);
        Assert.Equal(1000, achievement.ProgressCurrent);
    }

    [Fact]
    public void Signs1000Perform_TracksProgress_WhenBelow1000()
    {
        var achievement = CreateAchievement("SIGNS_1000_PERFORM", "Sign Language Star", "Successfully performed 1000 signs", "perform_1000_icon", 1000);
        int performModeSigns = 300;

        achievement.ProgressCurrent = Math.Min(performModeSigns, achievement.ProgressRequired);

        Assert.False(achievement.IsUnlocked);
        Assert.Equal(300, achievement.ProgressCurrent);
    }

    #endregion

    #region Quiz & Practice Achievements

    [Fact]
    public void QuizPerfect_Unlocks_WhenPerfectQuizCompleted()
    {
        var achievement = CreateAchievement("QUIZ_PERFECT", "Perfect Score", "Got 100% on a 100-question quiz", "quiz_master_icon", 1);
        bool perfectQuiz = true;

        if (perfectQuiz)
            SimulateUnlock(achievement, 1);

        Assert.True(achievement.IsUnlocked);
        Assert.Equal(1, achievement.ProgressCurrent);
    }

    [Fact]
    public void QuizPerfect_DoesNotUnlock_WhenNoPerfectQuiz()
    {
        var achievement = CreateAchievement("QUIZ_PERFECT", "Perfect Score", "Got 100% on a 100-question quiz", "quiz_master_icon", 1);
        bool perfectQuiz = false;

        achievement.ProgressCurrent = perfectQuiz ? 1 : 0;

        Assert.False(achievement.IsUnlocked);
        Assert.Equal(0, achievement.ProgressCurrent);
    }

    [Fact]
    public void PracticeHours10_Unlocks_WhenTotalHoursIsAtLeast10()
    {
        var achievement = CreateAchievement("PRACTICE_HOURS_10", "Dedicated Student", "Practiced for 10 hours", "time_icon", 10);
        var totalPracticeTime = TimeSpan.FromHours(10);

        if (totalPracticeTime.TotalHours >= 10)
            SimulateUnlock(achievement, (int)totalPracticeTime.TotalHours);

        Assert.True(achievement.IsUnlocked);
        Assert.Equal(10, achievement.ProgressCurrent);
    }

    [Fact]
    public void PracticeHours10_DoesNotUnlock_WhenBelowThreshold()
    {
        var achievement = CreateAchievement("PRACTICE_HOURS_10", "Dedicated Student", "Practiced for 10 hours", "time_icon", 10);
        var totalPracticeTime = TimeSpan.FromHours(5);

        if (totalPracticeTime.TotalHours >= 10)
            SimulateUnlock(achievement, (int)totalPracticeTime.TotalHours);

        Assert.False(achievement.IsUnlocked);
    }

    [Fact]
    public void PerfectSession_Unlocks_When50CorrectInARow()
    {
        var achievement = CreateAchievement("PERFECT_SESSION", "Perfect Session", "Got 50 signs correct in a row", "perfect_session_icon", 1);
        int correctInARow = 50;

        if (correctInARow >= 50)
            SimulateUnlock(achievement, 1);

        Assert.True(achievement.IsUnlocked);
        Assert.Equal(1, achievement.ProgressCurrent);
    }

    [Fact]
    public void PerfectSession_DoesNotUnlock_WhenBelow50InARow()
    {
        var achievement = CreateAchievement("PERFECT_SESSION", "Perfect Session", "Got 50 signs correct in a row", "perfect_session_icon", 1);
        int correctInARow = 30;

        if (correctInARow >= 50)
            SimulateUnlock(achievement, 1);

        Assert.False(achievement.IsUnlocked);
    }

    #endregion

    #region Social & Speed Achievements

    [Fact]
    public void SocialButterfly_Unlocks_WhenAchievementShared()
    {
        var achievement = CreateAchievement("SOCIAL_BUTTERFLY", "Social Butterfly", "Shared your first achievement", "social_icon", 1);
        bool hasShared = true;

        if (hasShared)
            SimulateUnlock(achievement, achievement.ProgressRequired);

        Assert.True(achievement.IsUnlocked);
        Assert.Equal(1, achievement.ProgressCurrent);
    }

    [Fact]
    public void SocialButterfly_DoesNotUnlock_WhenNothingShared()
    {
        var achievement = CreateAchievement("SOCIAL_BUTTERFLY", "Social Butterfly", "Shared your first achievement", "social_icon", 1);
        bool hasShared = false;

        if (hasShared)
            SimulateUnlock(achievement, achievement.ProgressRequired);

        Assert.False(achievement.IsUnlocked);
    }

    [Fact]
    public void RapidFire_Unlocks_WhenConditionMet()
    {
        var achievement = CreateAchievement("RAPID_FIRE", "Rapid Fire", "Answered 50 signs correctly in under 5 seconds each", "speed_icon", 5);

        SimulateUnlock(achievement, achievement.ProgressRequired);

        Assert.True(achievement.IsUnlocked);
        Assert.Equal(5, achievement.ProgressCurrent);
    }

    [Fact]
    public void SpeedMaster_Unlocks_WhenFastSessionCompleted()
    {
        var achievement = CreateAchievement("SPEED_MASTER", "Speed Master", "Completed a 100 Guess Mode session with average time under 3 seconds", "speed_master_icon", 1);
        bool hasFastSession = true;

        if (hasFastSession)
            SimulateUnlock(achievement, 1);

        Assert.True(achievement.IsUnlocked);
        Assert.Equal(1, achievement.ProgressCurrent);
    }

    [Fact]
    public void SpeedMaster_DoesNotUnlock_WhenNoFastSession()
    {
        var achievement = CreateAchievement("SPEED_MASTER", "Speed Master", "Completed a 100 Guess Mode session with average time under 3 seconds", "speed_master_icon", 1);
        bool hasFastSession = false;

        if (hasFastSession)
            SimulateUnlock(achievement, 1);

        Assert.False(achievement.IsUnlocked);
    }

    #endregion

    #region Multiplayer Achievements

    [Fact]
    public void PartyHost_Unlocks_WhenGameWith5PlusPlayers()
    {
        var achievement = CreateAchievement("PARTY_HOST", "Party Host", "Complete a multiplayer game with 5+ players", "party_icon", 1);
        int playerCount = 5;

        if (playerCount >= 5)
            SimulateUnlock(achievement, 1);

        Assert.True(achievement.IsUnlocked);
    }

    [Fact]
    public void PartyHost_DoesNotUnlock_WhenBelowMinPlayers()
    {
        var achievement = CreateAchievement("PARTY_HOST", "Party Host", "Complete a multiplayer game with 5+ players", "party_icon", 1);
        int playerCount = 3;

        if (playerCount >= 5)
            SimulateUnlock(achievement, 1);

        Assert.False(achievement.IsUnlocked);
    }

    [Fact]
    public void MultiplayerFirst_Unlocks_WhenFirstGameCompleted()
    {
        var achievement = CreateAchievement("MULTIPLAYER_FIRST", "Social Learner", "Complete your first multiplayer game", "multiplayer_icon", 1);
        bool firstGameCompleted = true;

        if (firstGameCompleted)
            SimulateUnlock(achievement, 1);

        Assert.True(achievement.IsUnlocked);
    }

    [Fact]
    public void MultiplayerFirst_DoesNotUnlock_WhenNoGamesPlayed()
    {
        var achievement = CreateAchievement("MULTIPLAYER_FIRST", "Social Learner", "Complete your first multiplayer game", "multiplayer_icon", 1);
        bool firstGameCompleted = false;

        if (firstGameCompleted)
            SimulateUnlock(achievement, 1);

        Assert.False(achievement.IsUnlocked);
    }

    [Fact]
    public void Multiplayer10_Unlocks_When10GamesCompleted()
    {
        var achievement = CreateAchievement("MULTIPLAYER_10", "Party Regular", "Complete 10 multiplayer games", "multiplayer_10_icon", 10);
        int gamesPlayed = 10;

        achievement.ProgressCurrent = gamesPlayed;
        if (gamesPlayed >= 10)
            SimulateUnlock(achievement, gamesPlayed);

        Assert.True(achievement.IsUnlocked);
        Assert.Equal(10, achievement.ProgressCurrent);
    }

    [Fact]
    public void Multiplayer10_TracksProgress_WhenBelow10()
    {
        var achievement = CreateAchievement("MULTIPLAYER_10", "Party Regular", "Complete 10 multiplayer games", "multiplayer_10_icon", 10);
        int gamesPlayed = 6;

        achievement.ProgressCurrent = gamesPlayed;

        Assert.False(achievement.IsUnlocked);
        Assert.Equal(6, achievement.ProgressCurrent);
    }

    [Fact]
    public void Multiplayer50_Unlocks_When50GamesCompleted()
    {
        var achievement = CreateAchievement("MULTIPLAYER_50", "Social Butterfly Pro", "Complete 50 multiplayer games", "multiplayer_50_icon", 50);
        int gamesPlayed = 50;

        achievement.ProgressCurrent = gamesPlayed;
        if (gamesPlayed >= 50)
            SimulateUnlock(achievement, gamesPlayed);

        Assert.True(achievement.IsUnlocked);
        Assert.Equal(50, achievement.ProgressCurrent);
    }

    [Fact]
    public void Multiplayer50_TracksProgress_WhenBelow50()
    {
        var achievement = CreateAchievement("MULTIPLAYER_50", "Social Butterfly Pro", "Complete 50 multiplayer games", "multiplayer_50_icon", 50);
        int gamesPlayed = 20;

        achievement.ProgressCurrent = gamesPlayed;

        Assert.False(achievement.IsUnlocked);
        Assert.Equal(20, achievement.ProgressCurrent);
    }

    [Fact]
    public void ChampionWin_Unlocks_When3ConsecutiveWins()
    {
        var achievement = CreateAchievement("CHAMPION_WIN", "Undefeated", "Win 3 multiplayer games in a row", "champion_icon", 3);
        int consecutiveWins = 3;

        if (consecutiveWins == 3)
            SimulateUnlock(achievement, 3);

        Assert.True(achievement.IsUnlocked);
        Assert.Equal(3, achievement.ProgressCurrent);
    }

    [Fact]
    public void ChampionWin_DoesNotUnlock_WhenBelow3Wins()
    {
        var achievement = CreateAchievement("CHAMPION_WIN", "Undefeated", "Win 3 multiplayer games in a row", "champion_icon", 3);
        int consecutiveWins = 2;

        if (consecutiveWins == 3)
            SimulateUnlock(achievement, 3);

        Assert.False(achievement.IsUnlocked);
    }

    [Fact]
    public void CloseCall_Unlocks_WhenWonBy1Point()
    {
        var achievement = CreateAchievement("CLOSE_CALL", "Nail Biter", "Win a multiplayer game by exactly 1 point", "close_call_icon", 1);
        bool wonByOnePoint = true;

        if (wonByOnePoint)
            SimulateUnlock(achievement, 1);

        Assert.True(achievement.IsUnlocked);
    }

    [Fact]
    public void CloseCall_DoesNotUnlock_WhenNoCloseWin()
    {
        var achievement = CreateAchievement("CLOSE_CALL", "Nail Biter", "Win a multiplayer game by exactly 1 point", "close_call_icon", 1);
        bool wonByOnePoint = false;

        if (wonByOnePoint)
            SimulateUnlock(achievement, 1);

        Assert.False(achievement.IsUnlocked);
    }

    [Fact]
    public void PerfectRoundAll_Unlocks_WhenAllPlayersCorrect()
    {
        var achievement = CreateAchievement("PERFECT_ROUND_ALL", "Team Harmony", "All players get the same sign correct", "harmony_icon", 1);
        bool allCorrect = true;

        if (allCorrect)
            SimulateUnlock(achievement, 1);

        Assert.True(achievement.IsUnlocked);
    }

    [Fact]
    public void PerfectRoundAll_DoesNotUnlock_WhenNotAllCorrect()
    {
        var achievement = CreateAchievement("PERFECT_ROUND_ALL", "Team Harmony", "All players get the same sign correct", "harmony_icon", 1);
        bool allCorrect = false;

        if (allCorrect)
            SimulateUnlock(achievement, 1);

        Assert.False(achievement.IsUnlocked);
    }

    [Fact]
    public void ComebackKing_Unlocks_WhenComebackVictory()
    {
        var achievement = CreateAchievement("COMEBACK_KING", "Comeback King", "Win a multiplayer game after being in last place", "comeback_icon", 1);
        bool comebackVictory = true;

        if (comebackVictory)
            SimulateUnlock(achievement, 1);

        Assert.True(achievement.IsUnlocked);
    }

    [Fact]
    public void ComebackKing_DoesNotUnlock_WhenNoComeback()
    {
        var achievement = CreateAchievement("COMEBACK_KING", "Comeback King", "Win a multiplayer game after being in last place", "comeback_icon", 1);
        bool comebackVictory = false;

        if (comebackVictory)
            SimulateUnlock(achievement, 1);

        Assert.False(achievement.IsUnlocked);
    }

    [Fact]
    public void PerfectMultiplayer_Unlocks_WhenAllSignsCorrectInGame()
    {
        var achievement = CreateAchievement("PERFECT_MULTIPLAYER", "Multiplayer Master", "Get all signs correct in a multiplayer game", "perfect_multi_icon", 1);
        bool perfectGame = true;

        if (perfectGame)
            SimulateUnlock(achievement, 1);

        Assert.True(achievement.IsUnlocked);
    }

    [Fact]
    public void PerfectMultiplayer_DoesNotUnlock_WhenNotPerfect()
    {
        var achievement = CreateAchievement("PERFECT_MULTIPLAYER", "Multiplayer Master", "Get all signs correct in a multiplayer game", "perfect_multi_icon", 1);
        bool perfectGame = false;

        if (perfectGame)
            SimulateUnlock(achievement, 1);

        Assert.False(achievement.IsUnlocked);
    }

    [Fact]
    public void PartyAnimal_Unlocks_When10DifferentGroupSizes()
    {
        var achievement = CreateAchievement("PARTY_ANIMAL", "Party Animal", "Play multiplayer games with 10 different group sizes", "party_animal_icon", 10);
        int distinctGroupSizes = 10;

        achievement.ProgressCurrent = distinctGroupSizes;
        if (distinctGroupSizes >= 10)
            SimulateUnlock(achievement, distinctGroupSizes);

        Assert.True(achievement.IsUnlocked);
        Assert.Equal(10, achievement.ProgressCurrent);
    }

    [Fact]
    public void PartyAnimal_TracksProgress_WhenBelow10GroupSizes()
    {
        var achievement = CreateAchievement("PARTY_ANIMAL", "Party Animal", "Play multiplayer games with 10 different group sizes", "party_animal_icon", 10);
        int distinctGroupSizes = 5;

        achievement.ProgressCurrent = distinctGroupSizes;

        Assert.False(achievement.IsUnlocked);
        Assert.Equal(5, achievement.ProgressCurrent);
    }

    #endregion

    #region Unlock Idempotency

    [Fact]
    public void Achievement_UnlockingTwice_DoesNotChangeDate()
    {
        var achievement = CreateAchievement("FIRST_SIGN", "First Steps", "Learnt your first sign", "first_sign_icon", 1);

        SimulateUnlock(achievement, 1);
        var firstUnlockDate = achievement.UnlockedDate;

        // Simulate a second unlock attempt
        SimulateUnlock(achievement, 1);

        Assert.True(achievement.IsUnlocked);
        Assert.Equal(firstUnlockDate, achievement.UnlockedDate);
    }

    #endregion

    #region Progress Capping

    [Fact]
    public void Achievement_ProgressCurrent_CapsAtProgressRequired()
    {
        var achievement = CreateAchievement("SIGNS_50", "Sign Master", "Learnt 50 signs", "fifty_signs_icon", 50);
        int signsLearned = 75;

        achievement.ProgressCurrent = Math.Min(signsLearned, achievement.ProgressRequired);

        Assert.Equal(50, achievement.ProgressCurrent);
    }

    [Theory]
    [InlineData("SIGNS_50", 50, 25, 25)]
    [InlineData("SIGNS_50", 50, 50, 50)]
    [InlineData("SIGNS_50", 50, 100, 50)]
    [InlineData("SIGNS_100", 100, 75, 75)]
    [InlineData("SIGNS_100", 100, 150, 100)]
    [InlineData("SIGNS_100_GUESS", 100, 60, 60)]
    [InlineData("SIGNS_1000_GUESS", 1000, 500, 500)]
    [InlineData("SIGNS_100_PERFORM", 100, 80, 80)]
    [InlineData("SIGNS_1000_PERFORM", 1000, 1200, 1000)]
    public void Achievement_ProgressCapping_WorksForVariousValues(string id, int required, int actual, int expected)
    {
        var achievement = new Achievement { Id = id, ProgressRequired = required };

        achievement.ProgressCurrent = Math.Min(actual, achievement.ProgressRequired);

        Assert.Equal(expected, achievement.ProgressCurrent);
    }

    #endregion
}
