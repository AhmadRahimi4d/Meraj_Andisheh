using System.Globalization;
using System.Text;

namespace MerajAndisheh.Core.Services;

/// <summary>
/// ساخت slug از عنوان (بخش ۳.۱۶): حروف فارسی مجاز، فاصله و نیم‌فاصله و علامت‌ها ← «-»،
/// ی/ک عربی ← فارسی، ارقام ← لاتین، اعراب و کشیده حذف؛ تکراری‌ها پسوند عددی می‌گیرند.
/// مثال: «مسیر اقتصاد» ← «مسیر-اقتصاد» و بار دوم «مسیر-اقتصاد-2».
/// </summary>
public static class SlugGenerator
{
    public const int DefaultMaxLength = 200;
    private const string FallbackSlug = "content";

    public static string Generate(string? title, IEnumerable<string> existingSlugs, int maxLength = DefaultMaxLength)
    {
        ArgumentNullException.ThrowIfNull(existingSlugs);

        var baseSlug = Slugify(title);
        if (baseSlug.Length == 0) baseSlug = FallbackSlug;
        if (baseSlug.Length > maxLength) baseSlug = baseSlug[..maxLength].TrimEnd('-');

        var existing = new HashSet<string>(existingSlugs, StringComparer.OrdinalIgnoreCase);
        if (!existing.Contains(baseSlug)) return baseSlug;

        for (var n = 2; ; n++)
        {
            var suffix = "-" + n.ToString(CultureInfo.InvariantCulture);
            var stem = baseSlug.Length + suffix.Length > maxLength
                ? baseSlug[..(maxLength - suffix.Length)].TrimEnd('-')
                : baseSlug;
            var candidate = stem + suffix;
            if (!existing.Contains(candidate)) return candidate;
        }
    }

    private static string Slugify(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var sb = new StringBuilder();
        var lastWasDash = true; // جلوگیری از «-» در ابتدا

        foreach (var original in input.Normalize(NormalizationForm.FormKC))
        {
            var c = Map(original);
            if (c == '\0') continue; // اعراب و کشیده

            if (char.IsLetterOrDigit(c))
            {
                sb.Append(char.ToLowerInvariant(c));
                lastWasDash = false;
            }
            else if (!lastWasDash)
            {
                sb.Append('-');
                lastWasDash = true;
            }
        }

        return sb.ToString().TrimEnd('-');
    }

    /// <summary>نگاشت نویسه‌ها؛ '\0' یعنی حذف شود.</summary>
    private static char Map(char c)
    {
        if (c == '\u064A' || c == '\u0649') return '\u06CC'; // ي و ى ← ی
        if (c == '\u0643') return '\u06A9';                    // ك ← ک
        if (c >= '\u0660' && c <= '\u0669') return (char)('0' + (c - '\u0660')); // ارقام عربی
        if (c >= '\u06F0' && c <= '\u06F9') return (char)('0' + (c - '\u06F0')); // ارقام فارسی
        if (c == '\u0640') return '\0';                        // کشیده
        if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) return '\0'; // اعراب
        return c;
    }
}
