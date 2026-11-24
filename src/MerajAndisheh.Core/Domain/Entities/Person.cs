namespace MerajAndisheh.Core.Domain.Entities;

/// <summary>شخصیت (مرتبط با محتوا).</summary>
public class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Bio { get; set; }

    /// <summary>کلید فایل عکس در IFileStorage.</summary>
    public string? PhotoKey { get; set; }
}
