using MerajAndisheh.Core.Domain;
using MerajAndisheh.Infrastructure.Identity;
using Xunit;

namespace MerajAndisheh.Tests;

public class TemporaryPasswordGeneratorTests
{
    private readonly TemporaryPasswordGenerator _generator = new();

    [Fact]
    public void Generated_IsLongEnough_AndMixesCharacterClasses()
    {
        for (var i = 0; i < 200; i++)
        {
            var value = _generator.Generate();

            Assert.True(value.Length >= AccountRules.MinPasswordLength);
            Assert.Equal(14, value.Length);
            Assert.Contains(value, c => char.IsUpper(c));
            Assert.Contains(value, c => char.IsLower(c));
            Assert.Contains(value, c => char.IsDigit(c));
        }
    }

    [Fact]
    public void Generated_AvoidsLookAlikeCharacters()
    {
        var all = string.Concat(Enumerable.Range(0, 300).Select(_ => _generator.Generate()));

        Assert.DoesNotContain('I', all);
        Assert.DoesNotContain('l', all);
        Assert.DoesNotContain('O', all);
        Assert.DoesNotContain('0', all);
        Assert.DoesNotContain('1', all);
    }

    [Fact]
    public void Generated_IsDifferentEveryTime()
    {
        var values = Enumerable.Range(0, 200).Select(_ => _generator.Generate()).ToList();

        Assert.Equal(values.Count, values.Distinct().Count());
    }
}
