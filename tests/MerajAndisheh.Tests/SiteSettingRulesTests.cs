using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Services;
using MerajAndisheh.Infrastructure.Data.Seed;
using Xunit;

namespace MerajAndisheh.Tests;

public class SiteSettingRulesTests
{
    [Fact]
    public void Definitions_CoverExactlyTheSeededSettings()
    {
        Assert.Equal(
            SeedData.SiteSettings.Keys.OrderBy(k => k, StringComparer.Ordinal),
            SiteSettingRules.All.Select(d => d.Key).OrderBy(k => k, StringComparer.Ordinal));
    }

    [Fact]
    public void SeedDefaults_PassTheirOwnRules()
    {
        foreach (var (key, value) in SeedData.SiteSettings)
        {
            var validation = SiteSettingRules.Validate(key, value);
            Assert.True(validation.IsValid, $"{key} = {value} → {validation.Error}");
        }
    }

    [Fact]
    public void UnknownKey_IsRejected()
    {
        Assert.Equal(SiteSettingError.UnknownKey, SiteSettingRules.Validate("Nope", "1").Error);
        Assert.Equal(SiteSettingError.UnknownKey, SiteSettingRules.Validate(null, "1").Error);
    }

    [Fact]
    public void Text_IsTrimmed_RequiredAndLimited()
    {
        Assert.Equal("عنوان", SiteSettingRules.Validate(SiteSettingKeys.SiteTitle, "  عنوان  ").Normalized);
        Assert.Equal(SiteSettingError.Required, SiteSettingRules.Validate(SiteSettingKeys.SiteTitle, "   ").Error);
        Assert.Equal(SiteSettingError.Required, SiteSettingRules.Validate(SiteSettingKeys.SiteTitle, null).Error);
        Assert.True(SiteSettingRules.Validate(SiteSettingKeys.SiteTitle, new string('ا', 100)).IsValid);
        Assert.Equal(SiteSettingError.TooLong, SiteSettingRules.Validate(SiteSettingKeys.SiteTitle, new string('ا', 101)).Error);
    }

    [Theory]
    [InlineData("1", "1")]
    [InlineData("0", "0")]
    [InlineData("۱", "1")]
    [InlineData(" ٠ ", "0")]
    public void Bool_AcceptsOnlyZeroAndOne_InAnyDigits(string raw, string expected)
    {
        var result = SiteSettingRules.Validate(SiteSettingKeys.PersonalizationDefaultEnabled, raw);

        Assert.True(result.IsValid);
        Assert.Equal(expected, result.Normalized);
    }

    [Theory]
    [InlineData("2")]
    [InlineData("true")]
    [InlineData("")]
    public void Bool_RejectsAnythingElse(string raw)
    {
        Assert.Equal(SiteSettingError.NotBool, SiteSettingRules.Validate(SiteSettingKeys.PersonalizationDefaultEnabled, raw).Error);
    }

    [Theory]
    [InlineData("5", "5")]
    [InlineData("۱۲", "12")]
    [InlineData("٣", "3")]
    [InlineData("007", "7")]
    [InlineData("100", "100")]   // سقف مجاز MinViews
    [InlineData("1", "1")]       // کف مجاز
    public void Int_AcceptsDigits_AndStoresLatin(string raw, string expected)
    {
        var result = SiteSettingRules.Validate(SiteSettingKeys.PersonalizationMinViews, raw);

        Assert.True(result.IsValid);
        Assert.Equal(expected, result.Normalized);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("-1")]
    [InlineData("+5")]
    [InlineData("1.5")]
    [InlineData("1 2")]
    [InlineData("99999999999")]
    public void Int_RejectsNonIntegers(string raw)
    {
        Assert.Equal(SiteSettingError.NotInteger, SiteSettingRules.Validate(SiteSettingKeys.PersonalizationMinViews, raw).Error);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("101")]
    public void Int_RejectsOutOfRange(string raw)
    {
        Assert.Equal(SiteSettingError.OutOfRange, SiteSettingRules.Validate(SiteSettingKeys.PersonalizationMinViews, raw).Error);
    }

    [Fact]
    public void Int_EmptyIsRequired()
    {
        Assert.Equal(SiteSettingError.Required, SiteSettingRules.Validate(SiteSettingKeys.PersonalizationMinViews, "  ").Error);
    }

    [Fact]
    public void CommentMaxLength_CannotExceedTheColumnSize()
    {
        Assert.True(SiteSettingRules.Validate(SiteSettingKeys.CommentsMaxLength, "1000").IsValid);
        Assert.Equal(SiteSettingError.OutOfRange, SiteSettingRules.Validate(SiteSettingKeys.CommentsMaxLength, "1001").Error);
    }
}
