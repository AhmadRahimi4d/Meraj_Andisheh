using MerajAndisheh.Core.Services;
using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>شماره‌ی موبایل با هر قالب رایج به 09xxxxxxxxx یکسان می‌شود.</summary>
public class PhoneNumberNormalizerTests
{
    [Theory]
    [InlineData("09123456789")]
    [InlineData("9123456789")]
    [InlineData("989123456789")]
    [InlineData("+989123456789")]
    [InlineData("00989123456789")]
    [InlineData("+98 912 345 6789")]
    [InlineData("0912-345-6789")]
    [InlineData("(0912) 345 6789")]
    [InlineData("۰۹۱۲۳۴۵۶۷۸۹")]
    [InlineData("٠٩١٢٣٤٥٦٧٨٩")]
    [InlineData("۰۹۱۲ ۳۴۵ ۶۷۸۹")]
    [InlineData("  09123456789  ")]
    public void TryNormalize_AcceptsCommonFormats(string input)
    {
        Assert.True(PhoneNumberNormalizer.TryNormalize(input, out var normalized));
        Assert.Equal("09123456789", normalized);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("09123")]                 // کوتاه
    [InlineData("0912345678")]            // ده رقم با صفر اول
    [InlineData("091234567890")]          // بلند
    [InlineData("08123456789")]           // موبایل با 9 شروع می‌شود
    [InlineData("02112345678")]           // تلفن ثابت
    [InlineData("0912345678a")]           // حرف
    [InlineData("ali@example.com")]
    [InlineData("+989123456789012")]
    [InlineData("98+9123456789")]         // + فقط اول مجاز است
    [InlineData("+")]
    public void TryNormalize_RejectsInvalidInput(string? input)
    {
        Assert.False(PhoneNumberNormalizer.TryNormalize(input, out var normalized));
        Assert.Null(normalized);
    }

    [Theory]
    [InlineData("09123456789", true)]
    [InlineData("۰۹۱۲", true)]           // ناقص هم «شبیه شماره» است (خطای شماره‌ی نادرست می‌گیرد، نه خطای رمز)
    [InlineData("+98 912 345 6789", true)]
    [InlineData("0912-345-6789", true)]
    [InlineData("ali@example.com", false)]
    [InlineData("ali", false)]
    [InlineData("ali123", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("+", false)]
    public void LooksLikePhone_DecidesTheLoginPath(string? input, bool expected)
    {
        Assert.Equal(expected, PhoneNumberNormalizer.LooksLikePhone(input));
    }
}
