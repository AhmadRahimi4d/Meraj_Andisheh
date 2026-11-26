using MerajAndisheh.Infrastructure.Otp;
using Microsoft.Extensions.Options;
using Xunit;

namespace MerajAndisheh.Tests;

public class OtpCodeHasherTests
{
    private const string Key = "unit-test-only-hash-key-0123456789-abcdef";

    private static OtpCodeHasher Create(string? key = Key)
        => new(Options.Create(new OtpOptions { HashKey = key }));

    [Fact]
    public void Hash_IsDeterministic_NotThePlainCode_And64HexChars()
    {
        var hasher = Create();

        var first = hasher.Hash("09121234567", "123456");
        var second = hasher.Hash("09121234567", "123456");

        Assert.Equal(first, second);
        Assert.NotEqual("123456", first);
        Assert.Equal(64, first.Length);
    }

    [Fact]
    public void Hash_DependsOnPhone_CodeAndKey()
    {
        var hasher = Create();
        var baseline = hasher.Hash("09121234567", "123456");

        Assert.NotEqual(baseline, hasher.Hash("09351234567", "123456"));
        Assert.NotEqual(baseline, hasher.Hash("09121234567", "123457"));
        Assert.NotEqual(baseline, Create("another-unit-test-key-0123456789-abcdef").Hash("09121234567", "123456"));
    }

    [Fact]
    public void Verify_AcceptsOnlyTheRightCodeAndPhone()
    {
        var hasher = Create();
        var stored = hasher.Hash("09121234567", "123456");

        Assert.True(hasher.Verify("09121234567", "123456", stored));
        Assert.False(hasher.Verify("09121234567", "654321", stored));
        Assert.False(hasher.Verify("09351234567", "123456", stored));
        Assert.False(hasher.Verify("09121234567", "123456", "short"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("too-short-key")]
    public void MissingOrShortKey_ThrowsWhenUsed(string? key)
    {
        var hasher = Create(key);

        Assert.Throws<InvalidOperationException>(() => hasher.Hash("09121234567", "123456"));
    }
}
