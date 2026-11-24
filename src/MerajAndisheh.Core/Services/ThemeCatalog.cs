namespace MerajAndisheh.Core.Services;

/// <summary>
/// فهرست ثابت تم‌های بصری (سه تم، بخش ۳.۱ سند). کلیدها با data-theme در tokens.css یکی هستند.
/// </summary>
public static class ThemeCatalog
{
    public const string Playful = "playful";
    public const string Modern = "modern";
    public const string Editorial = "editorial";

    /// <summary>تم پیش‌فرض وقتی رده‌ی سنی یا انتخابی معتبر نیست.</summary>
    public const string Default = Editorial;

    public static IReadOnlyList<string> All { get; } = [Playful, Modern, Editorial];

    public static bool IsValid(string? key)
    {
        return !string.IsNullOrWhiteSpace(key) && All.Contains(key.Trim().ToLowerInvariant());
    }

    /// <summary>کلید معتبر را کوچک‌شده برمی‌گرداند؛ ورودی نامعتبر ← تم پیش‌فرض.</summary>
    public static string Normalize(string? key)
    {
        return IsValid(key) ? key!.Trim().ToLowerInvariant() : Default;
    }
}
