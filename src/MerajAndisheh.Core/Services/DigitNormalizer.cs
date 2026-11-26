namespace MerajAndisheh.Core.Services;

/// <summary>تبدیل ارقام فارسی (۰-۹) و عربی (٠-٩) به لاتین (0-9). ورودی کاربر با هر دو نوع رقم پذیرفته می‌شود (بخش ۳.۱۶).</summary>
public static class DigitNormalizer
{
    public static string ToLatin(string? text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        var chars = text.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            var c = chars[i];
            if (c >= '\u06F0' && c <= '\u06F9') chars[i] = (char)('0' + (c - '\u06F0'));      // فارسی
            else if (c >= '\u0660' && c <= '\u0669') chars[i] = (char)('0' + (c - '\u0660')); // عربی
        }
        return new string(chars);
    }
}
