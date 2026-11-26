using System.Diagnostics.CodeAnalysis;

namespace MerajAndisheh.Core.Services;

/// <summary>
/// یکسان‌سازی شماره‌ی موبایل ایران به قالب 09xxxxxxxxx.
/// پذیرفته: 09123456789، 9123456789، 989123456789، +989123456789، 00989123456789؛
/// با ارقام فارسی یا عربی و با فاصله، خط تیره، پرانتز و نیم‌فاصله.
/// </summary>
public static class PhoneNumberNormalizer
{
    /// <summary>
    /// آیا ورودی «شبیه شماره» است؟ (فقط رقم، با «+» اختیاری اول). برای انتخاب مسیر OTP در فرم ورود؛
    /// معتبر بودن شماره را TryNormalize می‌سنجد. باید با login.js یکی باشد.
    /// </summary>
    public static bool LooksLikePhone(string? input)
    {
        var compact = Compact(input);
        if (compact.StartsWith('+')) compact = compact[1..];
        return compact.Length > 0 && compact.All(char.IsAsciiDigit);
    }

    public static bool TryNormalize(string? input, [NotNullWhen(true)] out string? normalized)
    {
        normalized = null;

        var digits = Compact(input);
        if (digits.StartsWith('+')) digits = digits[1..];
        if (digits.Length == 0 || !digits.All(char.IsAsciiDigit)) return false;

        string national; // ده رقم بدون صفر اول، مثل 9123456789
        if (digits.StartsWith("0098", StringComparison.Ordinal) && digits.Length == 14) national = digits[4..];
        else if (digits.StartsWith("98", StringComparison.Ordinal) && digits.Length == 12) national = digits[2..];
        else if (digits.StartsWith('0') && digits.Length == 11) national = digits[1..];
        else if (digits.Length == 10) national = digits;
        else return false;

        if (national.Length != 10 || national[0] != '9') return false;

        normalized = "0" + national;
        return true;
    }

    /// <summary>ارقام لاتین و حذف فاصله، خط تیره، پرانتز و نیم‌فاصله.</summary>
    private static string Compact(string? input)
    {
        var latin = DigitNormalizer.ToLatin(input);
        var buffer = new System.Text.StringBuilder(latin.Length);
        foreach (var c in latin)
        {
            if (char.IsWhiteSpace(c) || c is '-' or '(' or ')' or '\u200c' or '\u200e' or '\u200f') continue;
            buffer.Append(c);
        }
        return buffer.ToString();
    }
}
