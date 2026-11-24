using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Domain.Entities;
using MerajAndisheh.Core.Domain.Enums;

namespace MerajAndisheh.Core.Services;

/// <summary>
/// قواعد انتشار محتوا (بخش ۳.۳ سند). محتوا باید با Sources، Files و Topics بارگذاری شده باشد.
/// این سرویس هیچ متن فارسی ندارد؛ فقط کد خطا برمی‌گرداند.
/// </summary>
public sealed class ContentPublishValidator
{
    public PublishValidationResult Validate(ContentItem content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var errors = new List<PublishError>();

        // ۱) عنوان (حداکثر ۲۰۰) و خلاصه (حداکثر ۶۰۰)
        var title = content.Title?.Trim() ?? string.Empty;
        if (title.Length == 0) errors.Add(PublishError.TitleRequired);
        else if (title.Length > ContentLimits.TitleMaxLength) errors.Add(PublishError.TitleTooLong);

        var summary = content.Summary?.Trim() ?? string.Empty;
        if (summary.Length == 0) errors.Add(PublishError.SummaryRequired);
        else if (summary.Length > ContentLimits.SummaryMaxLength) errors.Add(PublishError.SummaryTooLong);

        // ۲) حداقل یک موضوع
        if (content.Topics.Count == 0) errors.Add(PublishError.TopicRequired);

        // ۳) حداقل یک منبع (عنوان منبع الزامی، نشانی اختیاری)
        if (!content.Sources.Any(s => !string.IsNullOrWhiteSpace(s.Title)))
            errors.Add(PublishError.SourceRequired);

        // ۴) قالب انتخاب شده باشد
        if (!Enum.IsDefined(content.Format)) errors.Add(PublishError.FormatRequired);

        // ۵) فایل اصلی یا لینک خارجی؛ برای مقاله متن (Body) کافی است
        var hasMainFileOrLink = content.Files.Any(f => f.Kind == FileKind.Main || f.StorageType == StorageType.External);
        if (!hasMainFileOrLink)
        {
            if (content.Format == ContentFormat.Article)
            {
                if (string.IsNullOrWhiteSpace(content.Body)) errors.Add(PublishError.ArticleBodyRequired);
            }
            else if (Enum.IsDefined(content.Format))
            {
                errors.Add(PublishError.MainFileOrLinkRequired);
            }
        }

        // ۶) تصویر شاخص
        if (string.IsNullOrWhiteSpace(content.ThumbnailKey)) errors.Add(PublishError.ThumbnailRequired);

        return new PublishValidationResult(errors);
    }
}
