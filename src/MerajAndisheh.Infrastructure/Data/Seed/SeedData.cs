using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Services;

namespace MerajAndisheh.Infrastructure.Data.Seed;

/// <summary>خود داده‌های اولیه (جدا از منطق اجرای Seed تا ویرایش داده‌ها ساده باشد).</summary>
public static class SeedData
{
    public sealed record AgeGroupSeed(string Key, string Name, int MinAge, int? MaxAge, string ThemeKey, int SortOrder);

    public sealed record TopicSeed(string Name, string[] Children);

    /// <summary>چهار رده‌ی سنی (بخش ۳.۱).</summary>
    public static IReadOnlyList<AgeGroupSeed> AgeGroups { get; } =
    [
        new("child", "کودک", 7, 11, ThemeCatalog.Playful, 1),
        new("teen", "نوجوان", 12, 15, ThemeCatalog.Modern, 2),
        new("youth", "جوان", 16, 20, ThemeCatalog.Modern, 3),
        new("adult", "بزرگسال", 21, null, ThemeCatalog.Editorial, 4)
    ];

    /// <summary>سه سطح اثرگذاری (قابل ویرایش در پنل).</summary>
    public static IReadOnlyList<string> ImpactLevels { get; } = ["کم", "متوسط", "زیاد"];

    /// <summary>موضوعات نمونه، درخت دو‌سطحی (۱۰ مورد).</summary>
    public static IReadOnlyList<TopicSeed> Topics { get; } =
    [
        new("تاریخ", ["ایران معاصر", "تاریخ جهان"]),
        new("رسانه", ["سواد رسانه‌ای", "تبلیغات"]),
        new("دشمن‌شناسی", []),
        new("اقتصاد", ["اقتصاد مقاومتی"]),
        new("فرهنگ و هنر", [])
    ];

    /// <summary>مقادیر پیش‌فرض SiteSetting (بخش ۶.۶).</summary>
    public static IReadOnlyDictionary<string, string> SiteSettings { get; } = new Dictionary<string, string>
    {
        [SiteSettingKeys.SiteTitle] = "معراج اندیشه",
        [SiteSettingKeys.SiteSubtitle] = "تبیین مستند و منبع‌دار برای همه‌ی سنین",
        [SiteSettingKeys.PersonalizationDefaultEnabled] = "1",
        [SiteSettingKeys.PersonalizationMinViews] = "3",
        [SiteSettingKeys.CommentsMaxLength] = "1000",
        [SiteSettingKeys.UploadMaxVideoMb] = "500",
        [SiteSettingKeys.UploadMaxOtherMb] = "100",
        [SiteSettingKeys.HomeItemsPerShelf] = "12"
    };
}
