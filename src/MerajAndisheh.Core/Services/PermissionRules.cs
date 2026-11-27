using MerajAndisheh.Core.Domain;

namespace MerajAndisheh.Core.Services;

/// <summary>قواعد خالص دسترسی بخش‌به‌بخش (بخش ۲ سند).</summary>
public static class PermissionRules
{
    public static bool IsValidKey(string? key)
        => key is not null && PermissionKeys.All.Contains(key, StringComparer.Ordinal);

    /// <summary>
    /// کلیدهای ورودی (مثلاً تیک‌های فرم) را یکتا و به ترتیب PermissionKeys برمی‌گرداند.
    /// اگر کلید ناشناخته‌ای باشد، null برمی‌گرداند (رد کامل، نه نادیده‌گرفتن بی‌صدا).
    /// </summary>
    public static IReadOnlyList<string>? Normalize(IEnumerable<string>? keys)
    {
        if (keys is null) return [];

        var set = new HashSet<string>(StringComparer.Ordinal);
        foreach (var key in keys)
        {
            if (!IsValidKey(key)) return null;
            set.Add(key);
        }

        return PermissionKeys.All.Where(set.Contains).ToList();
    }

    /// <summary>مدیر کل همه‌ی کلیدها را دارد؛ همکار فقط کلیدهای دادهشده؛ بقیه هیچ.</summary>
    public static bool HasPermission(bool isSuperAdmin, bool isStaff, IEnumerable<string> grantedKeys, string key)
    {
        if (!IsValidKey(key)) return false;
        if (isSuperAdmin) return true;
        return isStaff && grantedKeys.Contains(key, StringComparer.Ordinal);
    }
}
