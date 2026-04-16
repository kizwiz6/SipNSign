using com.kizwiz.signwiz.Enums;
using com.kizwiz.signwiz.Models;
using Xunit;

namespace SignWiz.Tests.Models;

public class GameParametersTests
{
    [Fact]
    public void NewGameParameters_HasDefaultValues()
    {
        var parameters = new GameParameters();

        Assert.False(parameters.IsMultiplayer);
        Assert.NotNull(parameters.Players);
        Assert.Empty(parameters.Players);
        Assert.Equal(com.kizwiz.signwiz.Constants.DEFAULT_PERFORM_QUESTIONS, parameters.QuestionsCount);
    }

    [Fact]
    public void GameParameters_CanConfigureMultiplayer()
    {
        var parameters = new GameParameters
        {
            IsMultiplayer = true,
            Players = new List<Player>
            {
                new Player { Name = "Player 1", IsMainPlayer = true },
                new Player { Name = "Player 2" }
            },
            QuestionsCount = 15
        };

        Assert.True(parameters.IsMultiplayer);
        Assert.Equal(2, parameters.Players.Count);
        Assert.Equal(15, parameters.QuestionsCount);
    }
}

public class RankedPlayerTests
{
    [Fact]
    public void NewRankedPlayer_HasDefaultValues()
    {
        var ranked = new RankedPlayer();

        Assert.Equal(0, ranked.Rank);
        Assert.Equal(string.Empty, ranked.Name);
        Assert.Equal(0, ranked.Score);
        Assert.False(ranked.IsWinner);
    }

    [Fact]
    public void RankedPlayer_CanSetWinner()
    {
        var ranked = new RankedPlayer
        {
            Rank = 1,
            Name = "Alice",
            Score = 15,
            IsWinner = true
        };

        Assert.Equal(1, ranked.Rank);
        Assert.Equal("Alice", ranked.Name);
        Assert.Equal(15, ranked.Score);
        Assert.True(ranked.IsWinner);
    }
}

public class SignModelTests
{
    [Fact]
    public void SignModel_CanBeCreated()
    {
        var sign = new SignModel
        {
            CorrectAnswer = "Hello",
            VideoPath = "hello.mp4",
            ImagePath = "hello.png",
            Language = SignLanguage.BSL,
            Category = SignCategory.Family,
            Choices = new List<string> { "Hello", "Goodbye", "Thank You", "Please" }
        };

        Assert.Equal("Hello", sign.CorrectAnswer);
        Assert.Equal(SignLanguage.BSL, sign.Language);
        Assert.Equal(SignCategory.Family, sign.Category);
        Assert.Equal(4, sign.Choices.Count);
        Assert.Contains("Hello", sign.Choices);
    }
}

public class SignPackTests
{
    [Fact]
    public void SignPack_NewPack_IsLocked()
    {
        var pack = new SignPack
        {
            Id = "animals",
            Name = "Animals Pack",
            Description = "Learn animal signs",
            Price = 0.99m,
            Categories = new List<SignCategory> { SignCategory.Animals }
        };

        Assert.False(pack.IsUnlocked);
        Assert.Equal(0.99m, pack.Price);
        Assert.Single(pack.Categories);
    }

    [Fact]
    public void SignPack_CanBeUnlocked()
    {
        var pack = new SignPack
        {
            Id = "animals",
            Name = "Animals Pack",
            Description = "Learn animal signs",
            Price = 0.99m,
            Categories = new List<SignCategory> { SignCategory.Animals },
            IsUnlocked = true
        };

        Assert.True(pack.IsUnlocked);
    }
}
