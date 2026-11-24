namespace MerajAndisheh.Core.Domain.Entities;

/// <summary>رده‌ی سنی (کودک، نوجوان، جوان، بزرگسال). بازه‌ها و نام‌ها از پنل قابل ویرایش‌اند.</summary>
public class AgeGroup
{
    public int Id { get; set; }

    /// <summary>کلید ثابت: child, teen, youth, adult.</summary>
    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public int MinAge { get; set; }

    /// <summary>null یعنی بدون سقف.</summary>
    public int? MaxAge { get; set; }

    /// <summary>کلید تم: playful, modern, editorial (ThemeCatalog).</summary>
    public string ThemeKey { get; set; } = string.Empty;

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
