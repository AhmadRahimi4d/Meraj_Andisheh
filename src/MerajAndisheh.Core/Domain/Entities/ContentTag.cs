namespace MerajAndisheh.Core.Domain.Entities;

/// <summary>جدول رابط محتوا و برچسب (کلید ترکیبی).</summary>
public class ContentTag
{
    public int ContentId { get; set; }
    public ContentItem Content { get; set; } = null!;

    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
