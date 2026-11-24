namespace MerajAndisheh.Core.Domain.Entities;

/// <summary>جدول رابط محتوا و موضوع (کلید ترکیبی).</summary>
public class ContentTopic
{
    public int ContentId { get; set; }
    public ContentItem Content { get; set; } = null!;

    public int TopicId { get; set; }
    public Topic Topic { get; set; } = null!;
}
