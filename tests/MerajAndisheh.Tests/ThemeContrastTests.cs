using System.Text.RegularExpressions;
using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>
/// معیار مرحله‌ی ۲: کنتراست متن به پس‌زمینه در هر سه تم حداقل ۴.۵ به ۱ (WCAG AA).
/// تست فایل واقعی wwwroot/css/tokens.css را می‌خواند، پس با تغییر رنگ‌ها خودکار دوباره بررسی می‌شود.
/// </summary>
public class ThemeContrastTests
{
    private const double TextMinimum = 4.5;
    private const double NonTextMinimum = 3.0;

    private static readonly Regex BlockRegex = new(
        "\\[data-theme=\"(?<name>[a-z]+)\"\\]\\s*\\{(?<body>[^{}]*)\\}", RegexOptions.Compiled);

    private static readonly Regex VarRegex = new(
        "--(?<key>[a-z-]+)\\s*:\\s*(?<value>#[0-9A-Fa-f]{6})\\s*;", RegexOptions.Compiled);

    private static Dictionary<string, Dictionary<string, string>> LoadThemes()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "MerajAndisheh.sln")))
        {
            dir = dir.Parent;
        }
        Assert.NotNull(dir);

        var path = Path.Combine(dir!.FullName, "src", "MerajAndisheh.Web", "wwwroot", "css", "tokens.css");
        Assert.True(File.Exists(path), "tokens.css not found: " + path);

        var css = File.ReadAllText(path);
        var themes = new Dictionary<string, Dictionary<string, string>>();
        foreach (Match block in BlockRegex.Matches(css))
        {
            var vars = new Dictionary<string, string>();
            foreach (Match v in VarRegex.Matches(block.Groups["body"].Value))
            {
                vars[v.Groups["key"].Value] = v.Groups["value"].Value;
            }
            themes[block.Groups["name"].Value] = vars;
        }
        return themes;
    }

    public static TheoryData<string> ThemeNames => new() { "playful", "modern", "editorial" };

    [Fact]
    public void TokensFile_DefinesAllThreeThemes()
    {
        var themes = LoadThemes();
        Assert.Contains("playful", themes.Keys);
        Assert.Contains("modern", themes.Keys);
        Assert.Contains("editorial", themes.Keys);
    }

    [Theory]
    [MemberData(nameof(ThemeNames))]
    public void Theme_TextColors_MeetAaContrast(string theme)
    {
        var t = LoadThemes()[theme];

        AssertRatio(t, "color-text", "color-bg", TextMinimum);
        AssertRatio(t, "color-text", "color-surface", TextMinimum);
        AssertRatio(t, "color-muted", "color-bg", TextMinimum);
        AssertRatio(t, "color-muted", "color-surface", TextMinimum);
        AssertRatio(t, "color-on-primary", "color-primary", TextMinimum);
        AssertRatio(t, "color-link", "color-bg", TextMinimum);
        AssertRatio(t, "color-link", "color-surface", TextMinimum);
    }

    [Theory]
    [MemberData(nameof(ThemeNames))]
    public void Theme_FocusAndControlBorders_MeetNonTextContrast(string theme)
    {
        var t = LoadThemes()[theme];

        AssertRatio(t, "color-focus", "color-bg", NonTextMinimum);
        AssertRatio(t, "color-focus", "color-surface", NonTextMinimum);
        AssertRatio(t, "color-control-border", "color-surface", NonTextMinimum);
    }

    private static void AssertRatio(Dictionary<string, string> theme, string foreground, string background, double minimum)
    {
        Assert.True(theme.ContainsKey(foreground), "missing variable: --" + foreground);
        Assert.True(theme.ContainsKey(background), "missing variable: --" + background);

        var ratio = ContrastCalculator.Ratio(theme[foreground], theme[background]);
        Assert.True(ratio >= minimum,
            $"--{foreground} ({theme[foreground]}) on --{background} ({theme[background]}) = {ratio:0.00}, required {minimum}");
    }
}
