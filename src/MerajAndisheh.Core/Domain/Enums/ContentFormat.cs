namespace MerajAndisheh.Core.Domain.Enums;

/// <summary>قالب محتوا (ستون tinyint). مقدار ۰ عمداً تعریف نشده تا «قالب انتخاب‌نشده» تشخیص داده شود.</summary>
public enum ContentFormat : byte
{
    Video = 1,
    Audio = 2,
    Article = 3,
    Image = 4,
    Pdf = 5,
    Presentation = 6,
    Book = 7,
    Film = 8,

    /// <summary>اسناد Word و مشابه (جدول ۳.۵ و معیار مرحله‌ی ۱۳ سند؛ در enum بخش ۶.۳ نبود و اینجا اضافه شد).</summary>
    Document = 9
}
