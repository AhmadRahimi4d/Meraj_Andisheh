namespace MerajAndisheh.Core.Domain.Entities;

/// <summary>میزان اثرگذاری (پیش‌فرض: کم، متوسط، زیاد).</summary>
public class ImpactLevel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
