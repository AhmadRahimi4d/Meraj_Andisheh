using MerajAndisheh.Core.Domain.Enums;

namespace MerajAndisheh.Core.Domain.Entities;

/// <summary>یک محتوا (ویدیو، مقاله، ...). هرگز حذف واقعی نمی‌شود؛ فقط بایگانی.</summary>
public class ContentItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;

    /// <summary>HTML پاک‌سازی‌شده (فقط برای قالب مقاله و توضیح بلند).</summary>
    public string? Body { get; set; }

    public ContentFormat Format { get; set; }

    public int? ImpactLevelId { get; set; }
    public ImpactLevel? ImpactLevel { get; set; }

    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public DateTime? PublishedAt { get; set; }

    /// <summary>مدت مطالعه یا تماشا (دقیقه).</summary>
    public int? ReadWatchMinutes { get; set; }

    public string? ThumbnailKey { get; set; }
    public bool IsFeatured { get; set; }
    public string LanguageCode { get; set; } = "fa";

    /// <summary>متن نرمال‌شده برای جستجو (بخش ۳.۷؛ از مرحله‌ی ۹ پر می‌شود).</summary>
    public string SearchText { get; set; } = string.Empty;

    public int ViewCount { get; set; }
    public int DownloadCount { get; set; }
    public int LikeCount { get; set; }

    /// <summary>سازنده. فعلاً عدد ساده است؛ کلید خارجی در مرحله‌ی ۴ اضافه می‌شود.</summary>
    public int CreatedById { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<ContentSource> Sources { get; set; } = [];
    public List<ContentFile> Files { get; set; } = [];
    public List<ContentTopic> Topics { get; set; } = [];
    public List<ContentTag> Tags { get; set; } = [];
    public List<ContentPerson> People { get; set; } = [];
    public List<ContentAgeGroup> AgeGroups { get; set; } = [];
}
