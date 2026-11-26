using System.Xml.Linq;
using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>
/// معیار مرحله‌ی ۵: در صفحه‌های عمومی هیچ لینک یا عبارت «admin» نیست؛ پنل فقط با نشانی مستقیم و برای همکار
/// و مدیر کل در دسترس است. (پنل در Areas است و اینجا بررسی نمی‌شود.)
/// </summary>
public class PublicPagesAdminReferenceTests
{
    private static readonly string WebDir = Path.Combine(SolutionPaths.Root(), "src", "MerajAndisheh.Web");

    [Fact]
    public void PublicViews_Scripts_AndStyles_DoNotMentionAdmin()
    {
        var files = new List<string>();
        files.AddRange(Directory.EnumerateFiles(Path.Combine(WebDir, "Pages"), "*.cshtml", SearchOption.AllDirectories));
        files.AddRange(Directory.EnumerateFiles(Path.Combine(WebDir, "wwwroot", "js"), "*.js"));
        files.AddRange(Directory.EnumerateFiles(Path.Combine(WebDir, "wwwroot", "css"), "*.css"));

        Assert.NotEmpty(files);

        var offenders = files
            .Where(f => File.ReadAllText(f).Contains("admin", StringComparison.OrdinalIgnoreCase))
            .Select(f => Path.GetRelativePath(WebDir, f))
            .ToList();

        Assert.True(offenders.Count == 0, "Public file mentions 'admin': " + string.Join(", ", offenders));
    }

    [Fact]
    public void ResourceTexts_DoNotMentionAdmin()
    {
        var resx = XDocument.Load(Path.Combine(WebDir, "Resources", "SharedResource.resx"));

        // فقط متن‌هایی که به کاربر نشان داده می‌شوند (value)، نه نام کلیدها
        var offenders = resx.Descendants("data")
            .Select(d => d.Element("value")?.Value ?? string.Empty)
            .Where(v => v.Contains("admin", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.Empty(offenders);
    }

    [Fact]
    public void AdminPage_LivesInAnArea_AndIsNotLinkedFromThePublicLayout()
    {
        Assert.True(File.Exists(Path.Combine(WebDir, "Areas", "Admin", "Pages", "Index.cshtml")));

        var layout = File.ReadAllText(Path.Combine(WebDir, "Pages", "Shared", "_Layout.cshtml"));
        Assert.DoesNotContain("/admin", layout, StringComparison.OrdinalIgnoreCase);
    }
}
