using System.Text.RegularExpressions;

namespace MerajAndisheh.Tests;

/// <summary>
/// منطق تشخیص رمز ثابت در متن فایل‌های سورس (جدا از خود تست‌ها، تا قابل تست مستقل باشد).
/// دو اصلاح نسبت به نسخه‌ی قبل:
/// ۱) الگوی «رشته‌ی اتصال» فقط روی فایل‌های json/config اجرا می‌شود؛ در کد C# فقط وقتی
///    داخل یک رشته‌ی دارای Server/Data Source باشد (تا «var password = Input.Password» گرفته نشود).
/// ۲) تخصیص‌های رشته‌ای که اسمشان Password دارد ولی مقدارشان رمز نیست (کلید claim و مسیر URL)
///    فقط با فهرست سفید دقیق (اسم + مقدار) مجازند؛ هر مقدار دیگری هنوز گرفته می‌شود.
/// </summary>
internal static class HardcodedPasswordScanner
{
    // Password = "..." یا TemporaryPassword = "..." در کد
    private static readonly Regex CodeAssignment = new(
        "(?i)\\b(?<name>\\w*password\\w*)\\s*=\\s*\"(?<value>[^\"]+)\"",
        RegexOptions.Compiled);

    // Password=abc یا Pwd=abc داخل رشته‌ی اتصال (فقط فایل‌های تنظیمات)
    private static readonly Regex ConnectionStringPart = new(
        "(?i)\\b(pwd|password)\\s*=\\s*[^;\"\\s]+",
        RegexOptions.Compiled);

    // رشته‌ی اتصال کامل داخل یک literal در کد: "Server=...;Password=..."
    private static readonly Regex ConnectionStringInCode = new(
        "(?i)\"(?=[^\"\\r\\n]*\\b(?:server|data source)\\s*=)[^\"\\r\\n]*\\b(?:pwd|password)\\s*=\\s*[^;\"\\s]+",
        RegexOptions.Compiled);

    // "SomethingPassword": "value" در JSON
    private static readonly Regex JsonValue = new(
        "(?i)\"[\\w:.]*password[\\w:.]*\"\\s*:\\s*\"[^\"]+\"",
        RegexOptions.Compiled);

    // فهرست سفید دقیق: «اسم|مقدار». این‌ها رمز نیستند.
    private static readonly HashSet<string> AllowedAssignments = new(StringComparer.Ordinal)
    {
        "MustChangePassword|ma:must_change_password",   // نوع claim (AppClaimTypes)
        "ChangePasswordPath|/account/change-password",   // مسیر صفحه (PasswordChangeGate)
    };

    private static readonly HashSet<string> ConfigExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".json", ".config",
    };

    public static bool ContainsHardcodedPassword(string extension, string text)
    {
        foreach (Match m in CodeAssignment.Matches(text))
        {
            var key = m.Groups["name"].Value + "|" + m.Groups["value"].Value;
            if (!AllowedAssignments.Contains(key)) return true;
        }

        if (ConfigExtensions.Contains(extension) && ConnectionStringPart.IsMatch(text)) return true;
        if (ConnectionStringInCode.IsMatch(text)) return true;
        if (JsonValue.IsMatch(text)) return true;

        return false;
    }
}
