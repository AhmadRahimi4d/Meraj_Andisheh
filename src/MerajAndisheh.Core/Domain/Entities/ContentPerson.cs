namespace MerajAndisheh.Core.Domain.Entities;

/// <summary>جدول رابط محتوا و شخصیت (کلید ترکیبی).</summary>
public class ContentPerson
{
    public int ContentId { get; set; }
    public ContentItem Content { get; set; } = null!;

    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;
}
