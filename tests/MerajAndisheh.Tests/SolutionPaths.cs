namespace MerajAndisheh.Tests;

/// <summary>پیدا کردن ریشه‌ی Solution برای تست‌هایی که فایل‌های پروژه را می‌خوانند.</summary>
public static class SolutionPaths
{
    public static string Root()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "MerajAndisheh.sln")))
        {
            dir = dir.Parent;
        }

        if (dir is null) throw new InvalidOperationException("MerajAndisheh.sln not found above " + AppContext.BaseDirectory);
        return dir.FullName;
    }
}
