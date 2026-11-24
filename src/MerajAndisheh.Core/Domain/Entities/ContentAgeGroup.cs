namespace MerajAndisheh.Core.Domain.Entities;

/// <summary>رده‌های هدف محتوا. محتوای بدون ردیف در این جدول یعنی «همه‌ی رده‌ها».</summary>
public class ContentAgeGroup
{
    public int ContentId { get; set; }
    public ContentItem Content { get; set; } = null!;

    public int AgeGroupId { get; set; }
    public AgeGroup AgeGroup { get; set; } = null!;
}
