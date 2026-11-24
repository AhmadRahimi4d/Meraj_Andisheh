namespace MerajAndisheh.Core.Services;

/// <summary>
/// دلیل رد شدن انتشار (قواعد ۳.۳). فقط کد است؛ متن فارسی هر کد در مرحله‌ی ۹ از فایل resx می‌آید.
/// </summary>
public enum PublishError
{
    TitleRequired = 1,
    TitleTooLong,
    SummaryRequired,
    SummaryTooLong,
    TopicRequired,
    SourceRequired,
    FormatRequired,
    MainFileOrLinkRequired,
    ArticleBodyRequired,
    ThumbnailRequired
}
