using System.Text.RegularExpressions;
using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>معیار مرحله‌ی ۴: در کد و مخزن هیچ رمز ثابتی وجود ندارد (بخش ۳.۱۵).</summary>
public class NoHardcodedPasswordsTests
{
    // Password = "..." یا TemporaryPassword = "..." در کد C#
    private static readonly Regex CodeAssignment = new("(?i)\\b\\w*password\\w*\\s*=\\s*\"[^\"]+\"", RegexOptions.Compiled);

    // Password=abc یا Pwd=abc داخل رشته‌ی اتصال
    private static readonly Regex ConnectionStringPart = new("(?i)\\b(pwd|password)\\s*=\\s*[^;\"\\s]+", RegexOptions.Compiled);

    // "SomethingPassword": "value" در فایل‌های JSON
    private static readonly Regex JsonValue = new("(?i)\"[\\w:.]*password[\\w:.]*\"\\s*:\\s*\"[^\"]+\"", RegexOptions.Compiled);

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
            if (CodeAssignment.IsMatch(text) || ConnectionStringPart.IsMatch(text) || JsonValue.IsMatch(text))
            {
                offenders.Add(Path.GetRelativePath(srcDir, file));
            }
        }

        Assert.True(offenders.Count == 0, "Possible hardcoded password in: " + string.Join(", ", offenders));
    }
}
