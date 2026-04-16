using com.kizwiz.signwiz.Models;
using Xunit;

namespace SignWiz.Tests.Models;

public class PlayerTests
{
    [Fact]
    public void NewPlayer_HasDefaultValues()
    {
        var player = new Player();

        Assert.Equal(string.Empty, player.Name);
        Assert.False(player.IsMainPlayer);
        Assert.Equal(0, player.Score);
        Assert.Equal(0, player.SelectedAnswer);
        Assert.Null(player.SelectedAnswerNumber);
        Assert.Null(player.SelectedAnswerText);
        Assert.False(player.HasAnswered);
        Assert.False(player.GotCurrentAnswerCorrect);
    }

    [Fact]
    public void AnswerStatus_WhenNotAnswered_ReturnsNotAnswered()
    {
        var player = new Player();

        Assert.Equal("Not answered", player.AnswerStatus);
    }

    [Fact]
    public void AnswerStatus_WhenCorrect_ReturnsCorrectWithCheckmark()
    {
        var player = new Player();
        player.HasAnswered = true;
        player.GotCurrentAnswerCorrect = true;

        Assert.Equal("Correct! ✓", player.AnswerStatus);
    }

    [Fact]
    public void AnswerStatus_WhenIncorrect_ReturnsIncorrectWithCross()
    {
        var player = new Player();
        player.HasAnswered = true;
        player.GotCurrentAnswerCorrect = false;

        Assert.Equal("Incorrect ✗", player.AnswerStatus);
    }

    [Fact]
    public void AnswerDisplayText_WhenNotAnswered_ReturnsWaiting()
    {
        var player = new Player();

        Assert.Equal("Waiting...", player.AnswerDisplayText);
    }

    [Fact]
    public void AnswerDisplayText_WhenAnsweredWithNumber_ShowsSelectedNumber()
    {
        var player = new Player();
        player.HasAnswered = true;
        player.SelectedAnswerNumber = 3;

        Assert.Equal("Selected: 3", player.AnswerDisplayText);
    }

    [Fact]
    public void AnswerDisplayText_WhenAnsweredCorrectly_ReturnsCorrectText()
    {
        var player = new Player();
        player.HasAnswered = true;
        player.GotCurrentAnswerCorrect = true;

        Assert.Equal("Correct! ✓", player.AnswerDisplayText);
    }

    [Fact]
    public void AnswerDisplayText_WhenAnsweredIncorrectly_ReturnsIncorrectText()
    {
        var player = new Player();
        player.HasAnswered = true;
        player.GotCurrentAnswerCorrect = false;

        Assert.Equal("Incorrect ✗", player.AnswerDisplayText);
    }

    [Fact]
    public void IndicatorColor_WhenNotAnswered_ReturnsGray()
    {
        var player = new Player();

        Assert.Equal(Colors.Gray, player.IndicatorColor);
    }

    [Fact]
    public void IndicatorColor_WhenCorrect_ReturnsGreen()
    {
        var player = new Player();
        player.HasAnswered = true;
        player.GotCurrentAnswerCorrect = true;

        Assert.Equal(Color.FromArgb("#22C55E"), player.IndicatorColor);
    }

    [Fact]
    public void IndicatorColor_WhenIncorrect_ReturnsRed()
    {
        var player = new Player();
        player.HasAnswered = true;
        player.GotCurrentAnswerCorrect = false;

        Assert.Equal(Color.FromArgb("#EF4444"), player.IndicatorColor);
    }

    #region RecordGuessAnswer Tests

    [Fact]
    public void RecordGuessAnswer_SetsAllProperties()
    {
        var player = new Player { Name = "Alice" };

        player.RecordGuessAnswer(2, "Hello", true);

        Assert.Equal(2, player.SelectedAnswerNumber);
        Assert.Equal("Hello", player.SelectedAnswerText);
        Assert.Equal(2, player.SelectedAnswer);
        Assert.True(player.HasAnswered);
        Assert.True(player.GotCurrentAnswerCorrect);
    }

    [Fact]
    public void RecordGuessAnswer_DoesNotChangeScore()
    {
        var player = new Player { Name = "Alice" };
        int originalScore = player.Score;

        player.RecordGuessAnswer(1, "Goodbye", true);

        Assert.Equal(originalScore, player.Score);
    }

    [Fact]
    public void RecordGuessAnswer_IncorrectAnswer_SetsGotCurrentAnswerCorrectFalse()
    {
        var player = new Player { Name = "Bob" };

        player.RecordGuessAnswer(3, "Wrong", false);

        Assert.True(player.HasAnswered);
        Assert.False(player.GotCurrentAnswerCorrect);
    }

    #endregion

    #region RecordAnswer Tests

    [Fact]
    public void RecordAnswer_FirstCorrectAnswer_IncrementsScore()
    {
        var player = new Player { Name = "Alice" };

        player.RecordAnswer(true);

        Assert.Equal(1, player.Score);
        Assert.True(player.HasAnswered);
        Assert.True(player.GotCurrentAnswerCorrect);
    }

    [Fact]
    public void RecordAnswer_FirstIncorrectAnswer_DoesNotChangeScore()
    {
        var player = new Player { Name = "Alice" };

        player.RecordAnswer(false);

        Assert.Equal(0, player.Score);
        Assert.True(player.HasAnswered);
        Assert.False(player.GotCurrentAnswerCorrect);
    }

    [Fact]
    public void RecordAnswer_ChangeFromCorrectToIncorrect_DecrementsScore()
    {
        var player = new Player { Name = "Alice" };

        // First answer: correct
        player.RecordAnswer(true);
        Assert.Equal(1, player.Score);

        // Change to incorrect
        player.RecordAnswer(false);
        Assert.Equal(0, player.Score);
        Assert.False(player.GotCurrentAnswerCorrect);
    }

    [Fact]
    public void RecordAnswer_ChangeFromIncorrectToCorrect_IncrementsScore()
    {
        var player = new Player { Name = "Alice" };

        // First answer: incorrect
        player.RecordAnswer(false);
        Assert.Equal(0, player.Score);

        // Change to correct
        player.RecordAnswer(true);
        Assert.Equal(1, player.Score);
        Assert.True(player.GotCurrentAnswerCorrect);
    }

    [Fact]
    public void RecordAnswer_SameAnswerTwice_NoScoreChange()
    {
        var player = new Player { Name = "Alice" };

        player.RecordAnswer(true);
        Assert.Equal(1, player.Score);

        // Same answer again
        player.RecordAnswer(true);
        Assert.Equal(1, player.Score);
    }

    #endregion

    #region ResetForNewSign Tests

    [Fact]
    public void ResetForNewSign_ClearsAnswerState()
    {
        var player = new Player { Name = "Alice" };
        player.RecordGuessAnswer(2, "Hello", true);

        player.ResetForNewSign();

        Assert.False(player.HasAnswered);
        Assert.False(player.GotCurrentAnswerCorrect);
        Assert.Null(player.SelectedAnswerNumber);
        Assert.Null(player.SelectedAnswerText);
        Assert.Equal(0, player.SelectedAnswer);
    }

    [Fact]
    public void ResetForNewSign_DoesNotResetScore()
    {
        var player = new Player { Name = "Alice" };
        player.RecordAnswer(true);
        Assert.Equal(1, player.Score);

        player.ResetForNewSign();

        Assert.Equal(1, player.Score);
    }

    [Fact]
    public void ResetForNewSign_DoesNotResetName()
    {
        var player = new Player { Name = "Alice", IsMainPlayer = true };

        player.ResetForNewSign();

        Assert.Equal("Alice", player.Name);
        Assert.True(player.IsMainPlayer);
    }

    #endregion

    #region PropertyChanged Tests

    [Fact]
    public void Score_RaisesPropertyChanged()
    {
        var player = new Player();
        var raised = false;
        player.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Player.Score))
                raised = true;
        };

        player.Score = 5;

        Assert.True(raised);
    }

    [Fact]
    public void HasAnswered_RaisesPropertyChanged()
    {
        var player = new Player();
        var raisedProperties = new List<string>();
        player.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != null)
                raisedProperties.Add(args.PropertyName);
        };

        player.HasAnswered = true;

        Assert.Contains(nameof(Player.HasAnswered), raisedProperties);
        Assert.Contains(nameof(Player.AnswerStatus), raisedProperties);
        Assert.Contains(nameof(Player.IndicatorColor), raisedProperties);
    }

    [Fact]
    public void GotCurrentAnswerCorrect_RaisesPropertyChanged()
    {
        var player = new Player();
        var raisedProperties = new List<string>();
        player.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != null)
                raisedProperties.Add(args.PropertyName);
        };

        player.GotCurrentAnswerCorrect = true;

        Assert.Contains(nameof(Player.GotCurrentAnswerCorrect), raisedProperties);
        Assert.Contains(nameof(Player.AnswerStatus), raisedProperties);
        Assert.Contains(nameof(Player.IndicatorColor), raisedProperties);
    }

    #endregion
}
