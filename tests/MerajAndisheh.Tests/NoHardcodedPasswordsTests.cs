using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>معیار مرحله‌ی ۴: در کد و مخزن هیچ رمز ثابتی وجود ندارد (بخش ۳.۱۵).</summary>
public class NoHardcodedPasswordsTests
{
    [Fact]
    public void SourceFiles_HaveNoHardcodedPasswords()
    {
        var srcDir = Path.Combine(SolutionPaths.Root(), "src");
        var extensions = new[] { ".cs", ".cshtml", ".json", ".config", ".js" };

        var files = Directory.EnumerateFiles(srcDir, "*.*", SearchOption.AllDirectories)
            .Where(f => extensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                        && !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                        && !f.Contains($"{Path.DirectorySeparatorChar}lib{Path.DirectorySeparatorChar}"))
            .ToList();

        Assert.NotEmpty(files);

        var offenders = new List<string>();
        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            if (HardcodedPasswordScanner.ContainsHardcodedPassword(Path.GetExtension(file), text))
            {
                offenders.Add(Path.GetRelativePath(srcDir, file));
            }
        }

        Assert.True(offenders.Count == 0, "Possible hardcoded password in: " + string.Join(", ", offenders));
    }

    // تضمین اینکه سخت‌گیری تست کم نشده: این نمونه‌ها باید همچنان گرفته شوند.
    [Theory]
    [InlineData(".cs", "var Password = \"Admin1234\";")]
    [InlineData(".cs", "public const string TemporaryPassword = \"Temp#1234\";")]
    [InlineData(".cs", "public const string MustChangePassword = \"Admin1234\";")]
    [InlineData(".cs", "var cs = \"Server=.;Database=x;User Id=sa;Password=abc123;\";")]
    [InlineData(".json", "{\"ConnectionStrings\":{\"Default\":\"Server=.;Pwd=abc\"}}")]
    [InlineData(".json", "{\"SuperAdmin:TemporaryPassword\": \"abc\"}")]
    public void Scanner_FlagsRealHardcodedPasswords(string extension, string text)
    {
        Assert.True(HardcodedPasswordScanner.ContainsHardcodedPassword(extension, text));
    }

    // نمونه‌های بی‌خطر که قبلاً به‌اشتباه گرفته می‌شدند.
    [Theory]
    [InlineData(".cs", "public const string MustChangePassword = \"ma:must_change_password\";")]
    [InlineData(".cs", "public const string ChangePasswordPath = \"/account/change-password\";")]
    [InlineData(".cs", "var password = Input.Password ?? string.Empty;")]
    [InlineData(".json", "{\"Logging\":{\"LogLevel\":{\"Default\":\"Information\"}}}")]
    public void Scanner_IgnoresHarmlessLines(string extension, string text)
    {
        Assert.False(HardcodedPasswordScanner.ContainsHardcodedPassword(extension, text));
    }
}
