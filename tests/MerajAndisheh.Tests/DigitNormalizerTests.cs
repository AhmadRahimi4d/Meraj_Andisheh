using MerajAndisheh.Core.Services;
using Xunit;

namespace MerajAndisheh.Tests;

public class DigitNormalizerTests
{
    [Theory]
    [InlineData("۰۱۲۳۴۵۶۷۸۹", "0123456789")]
    [InlineData("٠١٢٣٤٥٦٧٨٩", "0123456789")]
    [InlineData("0123456789", "0123456789")]
    [InlineData("کد ۱۲۳ abc", "کد 123 abc")]
    [InlineData("", "")]
    public void ToLatin_ConvertsPersianAndArabicDigits(string input, string expected)
    {
        Assert.Equal(expected, DigitNormalizer.ToLatin(input));
    }

    [Fact]
    public void ToLatin_Null_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, DigitNormalizer.ToLatin(null));
    }
}
