using MerajAndisheh.Core.Services;
using Xunit;

namespace MerajAndisheh.Tests;

public class ThemeCatalogTests
{
    [Theory]
    [InlineData("playful", "playful")]
    [InlineData("MODERN", "modern")]
    [InlineData("  Editorial ", "editorial")]
    public void Normalize_ValidKey_ReturnsLowerCaseKey(string input, string expected)
    {
        Assert.Equal(expected, ThemeCatalog.Normalize(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("dark")]
    [InlineData("\"><script>")]
    public void Normalize_InvalidKey_ReturnsDefault(string? input)
    {
        Assert.Equal(ThemeCatalog.Default, ThemeCatalog.Normalize(input));
    }

    [Fact]
    public void All_ContainsExactlyThreeThemes()
    {
        Assert.Equal(3, ThemeCatalog.All.Count);
    }
}
