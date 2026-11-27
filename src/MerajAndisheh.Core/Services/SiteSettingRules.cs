using System.Globalization;
using MerajAndisheh.Core.Domain;

namespace MerajAndisheh.Core.Services;

/// <summary>
/// تنظیمات قابل ویرایش سایت (فهرست بخش ۶.۶) و قاعده‌ی اعتبارسنجی هر کدام. منطق خالص و بدون دیتابیس.
/// ارقام فارسی و عربی هم پذیرفته می‌شود (بخش ۳.۱۶). مقدار ذخیره‌شده همیشه لاتین است.
/// </summary>
public static class SiteSettingRules
{
    public static IReadOnlyList<SiteSettingDefinition> All { get; } =
    [
        new(SiteSettingKeys.SiteTitle, SiteSettingKind.Text, 1, 100),
        new(SiteSettingKeys.SiteSubtitle, SiteSettingKind.Text, 1, 200),
        new(SiteSettingKeys.PersonalizationDefaultEnabled, SiteSettingKind.Bool, 0, 1),
        new(SiteSettingKeys.PersonalizationMinViews, SiteSettingKind.Int, 1, 100),
        // ستون متن نظر nvarchar(1000) است؛ پس سقف بیشتر از ۱۰۰۰ ممکن نیست (بخش ۳.۱۲ و ۶.۴)
        new(SiteSettingKeys.CommentsMaxLength, SiteSettingKind.Int, 50, 1000),
        new(SiteSettingKeys.UploadMaxVideoMb, SiteSettingKind.Int, 1, 5000),
        new(SiteSettingKeys.UploadMaxOtherMb, SiteSettingKind.Int, 1, 1000),
        new(SiteSettingKeys.HomeItemsPerShelf, SiteSettingKind.Int, 4, 48),
    ];

    public static SiteSettingDefinition? Find(string? key)
        => All.FirstOrDefault(d => string.Equals(d.Key, key, StringComparison.Ordinal));

    public static SiteSettingValidation Validate(string? key, string? raw)
    {
        var definition = Find(key);
        if (definition is null) return new SiteSettingValidation(null, SiteSettingError.UnknownKey);

        return definition.Kind switch
        {
            SiteSettingKind.Text => ValidateText(definition, raw),
            SiteSettingKind.Bool => ValidateBool(raw),
            _ => ValidateInt(definition, raw)
        };
    }

    private static SiteSettingValidation ValidateText(SiteSettingDefinition definition, string? raw)
    {
        var text = raw?.Trim() ?? string.Empty;
        if (text.Length < definition.Min) return new SiteSettingValidation(null, SiteSettingError.Required);
        if (text.Length > definition.Max) return new SiteSettingValidation(null, SiteSettingError.TooLong);
        return new SiteSettingValidation(text, null);
    }

    private static SiteSettingValidation ValidateBool(string? raw)
    {
        var text = DigitNormalizer.ToLatin(raw).Trim();
        return text is "0" or "1"
            ? new SiteSettingValidation(text, null)
            : new SiteSettingValidation(null, SiteSettingError.NotBool);
    }

    private static SiteSettingValidation ValidateInt(SiteSettingDefinition definition, string? raw)
    {
        var text = DigitNormalizer.ToLatin(raw).Trim();
        if (text.Length == 0) return new SiteSettingValidation(null, SiteSettingError.Required);

        // NumberStyles.None: فقط رقم؛ بدون علامت، فاصله و ممیز
        if (!int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var number))
            return new SiteSettingValidation(null, SiteSettingError.NotInteger);

        if (number < definition.Min || number > definition.Max)
            return new SiteSettingValidation(null, SiteSettingError.OutOfRange);

        return new SiteSettingValidation(number.ToString(CultureInfo.InvariantCulture), null);
    }
}
