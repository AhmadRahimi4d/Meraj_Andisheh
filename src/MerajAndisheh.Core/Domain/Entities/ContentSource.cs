namespace MerajAndisheh.Core.Domain.Entities;

/// <summary>منبع و مستند هر محتوا. حداقل یک منبع برای انتشار الزامی است.</summary>
public class ContentSource
{
    public int Id { get; set; }
    public int ContentId { get; set; }

    /// <summary>عنوان منبع (الزامی).</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>نشانی منبع (اختیاری).</summary>
    public string? Url { get; set; }

    public string? Note { get; set; }
    public int SortOrder { get; set; }
}
