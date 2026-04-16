using com.kizwiz.signwiz.Converters;
using System.Globalization;
using Xunit;

namespace SignWiz.Tests.Converters;

public class ConverterTests
{
    #region InverseBoolConverter

    [Fact]
    public void InverseBoolConverter_True_ReturnsFalse()
    {
        var converter = new InverseBoolConverter();
        var result = converter.Convert(true, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.Equal(false, result);
    }

    [Fact]
    public void InverseBoolConverter_False_ReturnsTrue()
    {
        var converter = new InverseBoolConverter();
        var result = converter.Convert(false, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.Equal(true, result);
    }

    [Fact]
    public void InverseBoolConverter_NonBool_ReturnsNull()
    {
        var converter = new InverseBoolConverter();
        var result = converter.Convert("not a bool", typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.Null(result);
    }

    [Fact]
    public void InverseBoolConverter_ConvertBack_InvertsValue()
    {
        var converter = new InverseBoolConverter();
        var result = converter.ConvertBack(true, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.Equal(false, result);
    }

    #endregion

    #region BoolToColorConverter

    [Fact]
    public void BoolToColorConverter_True_ReturnsGreen()
    {
        var converter = new BoolToColorConverter();
        var result = converter.Convert(true, typeof(Color), null, CultureInfo.InvariantCulture);
        Assert.Equal(Colors.Green, result);
    }

    [Fact]
    public void BoolToColorConverter_False_ReturnsGray()
    {
        var converter = new BoolToColorConverter();
        var result = converter.Convert(false, typeof(Color), null, CultureInfo.InvariantCulture);
        Assert.Equal(Colors.Gray, result);
    }

    [Fact]
    public void BoolToColorConverter_NonBool_ReturnsNull()
    {
        var converter = new BoolToColorConverter();
        var result = converter.Convert("not a bool", typeof(Color), null, CultureInfo.InvariantCulture);
        Assert.Null(result);
    }

    #endregion

    #region BoolToTextColorConverter

    [Fact]
    public void BoolToTextColorConverter_True_ReturnsWhite()
    {
        var converter = new BoolToTextColorConverter();
        var result = converter.Convert(true, typeof(Color), null, CultureInfo.InvariantCulture);
        Assert.Equal(Colors.White, result);
    }

    [Fact]
    public void BoolToTextColorConverter_False_ReturnsDarkGray()
    {
        var converter = new BoolToTextColorConverter();
        var result = converter.Convert(false, typeof(Color), null, CultureInfo.InvariantCulture);
        Assert.Equal(Colors.DarkGray, result);
    }

    #endregion

    #region BoolToOpacityConverter

    [Fact]
    public void BoolToOpacityConverter_True_Returns1()
    {
        var converter = new BoolToOpacityConverter();
        var result = converter.Convert(true, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(1.0, result);
    }

    [Fact]
    public void BoolToOpacityConverter_False_ReturnsHalf()
    {
        var converter = new BoolToOpacityConverter();
        var result = converter.Convert(false, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(0.5, result);
    }

    [Fact]
    public void BoolToOpacityConverter_NonBool_ReturnsHalf()
    {
        var converter = new BoolToOpacityConverter();
        var result = converter.Convert("not a bool", typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(0.5, result);
    }

    #endregion

    #region BoolToAchievementStatusConverter

    [Fact]
    public void BoolToAchievementStatusConverter_True_ReturnsTrophy()
    {
        var converter = new BoolToAchievementStatusConverter();
        var result = converter.Convert(true, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal("🏆", result);
    }

    [Fact]
    public void BoolToAchievementStatusConverter_False_ReturnsEmpty()
    {
        var converter = new BoolToAchievementStatusConverter();
        var result = converter.Convert(false, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void BoolToAchievementStatusConverter_Null_ReturnsEmpty()
    {
        var converter = new BoolToAchievementStatusConverter();
        var result = converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal(string.Empty, result);
    }

    #endregion

    #region ButtonEnabledConverter

    [Fact]
    public void ButtonEnabledConverter_NotProcessingAndActive_ReturnsTrue()
    {
        var converter = new ButtonEnabledConverter();
        var result = converter.Convert(new object[] { false, true }, typeof(bool), null!, CultureInfo.InvariantCulture);
        Assert.Equal(true, result);
    }

    [Fact]
    public void ButtonEnabledConverter_ProcessingAndActive_ReturnsFalse()
    {
        var converter = new ButtonEnabledConverter();
        var result = converter.Convert(new object[] { true, true }, typeof(bool), null!, CultureInfo.InvariantCulture);
        Assert.Equal(false, result);
    }

    [Fact]
    public void ButtonEnabledConverter_NotProcessingAndInactive_ReturnsFalse()
    {
        var converter = new ButtonEnabledConverter();
        var result = converter.Convert(new object[] { false, false }, typeof(bool), null!, CultureInfo.InvariantCulture);
        Assert.Equal(false, result);
    }

    [Fact]
    public void ButtonEnabledConverter_InvalidInput_ReturnsTrue()
    {
        var converter = new ButtonEnabledConverter();
        var result = converter.Convert(new object[] { "invalid" }, typeof(bool), null!, CultureInfo.InvariantCulture);
        Assert.Equal(true, result);
    }

    #endregion
}
