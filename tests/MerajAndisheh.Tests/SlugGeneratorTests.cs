using MerajAndisheh.Core.Services;
using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>مثال بخش ۳.۱۶: «مسیر اقتصاد» ← «مسیر-اقتصاد» و «مسیر-اقتصاد-2».</summary>
public class SlugGeneratorTests
{
    [Fact]
    public void FirstTitle_BecomesHyphenatedPersianSlug()
    {
        var slug = SlugGenerator.Generate("مسیر اقتصاد", []);

        Assert.Equal("مسیر-اقتصاد", slug);
    }

    [Fact]
    public void DuplicateTitle_GetsNumericSuffix_FromTwo()
    {
        var first = SlugGenerator.Generate("مسیر اقتصاد", []);
        var second = SlugGenerator.Generate("مسیر اقتصاد", [first]);
        var third = SlugGenerator.Generate("مسیر اقتصاد", [first, second]);

        Assert.Equal("مسیر-اقتصاد", first);
        Assert.Equal("مسیر-اقتصاد-2", second);
        Assert.Equal("مسیر-اقتصاد-3", third);
    }

    [Fact]
    public void ArabicLettersAndDigits_AreNormalized()
    {
        // ك و ي عربی، ارقام فارسی
        Assert.Equal("کتاب-123", SlugGenerator.Generate("كتاب ۱۲۳", []));
        Assert.Equal("کتاب-123", SlugGenerator.Generate("كتاب ١٢٣", []));
        Assert.Equal("علی", SlugGenerator.Generate("علي", []));
    }

    [Fact]
    public void HalfSpace_And_Punctuation_BecomeSingleHyphen()
    {
        Assert.Equal("می-شود", SlugGenerator.Generate("می\u200Cشود", []));
        Assert.Equal("سواد-رسانه", SlugGenerator.Generate("  سواد،  رسانه!؟  ", []));
    }

    [Fact]
    public void Diacritics_And_Tatweel_AreRemoved()
    {
        Assert.Equal("علم", SlugGenerator.Generate("عِلْم", []));
        Assert.Equal("علم", SlugGenerator.Generate("عـلـم", []));
    }

    [Fact]
    public void LatinText_IsLowerCased()
    {
        Assert.Equal("hello-world-2", SlugGenerator.Generate("Hello, World!", ["hello-world"]));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("!!!")]
    public void EmptyOrSymbolOnlyTitle_UsesFallback(string? title)
    {
        Assert.Equal("content", SlugGenerator.Generate(title, []));
    }

    [Fact]
    public void LongTitle_IsTruncated_AndSuffixStillFits()
    {
        var title = string.Join(' ', Enumerable.Repeat("کلمه", 100));

        var first = SlugGenerator.Generate(title, [], maxLength: 50);
        var second = SlugGenerator.Generate(title, [first], maxLength: 50);

        Assert.True(first.Length <= 50);
        Assert.True(second.Length <= 50);
        Assert.EndsWith("-2", second);
        Assert.NotEqual(first, second);
    }

    [Fact]
    public void ExistingSlugs_AreComparedCaseInsensitively()
    {
        Assert.Equal("test-2", SlugGenerator.Generate("test", ["TEST"]));
    }
}
