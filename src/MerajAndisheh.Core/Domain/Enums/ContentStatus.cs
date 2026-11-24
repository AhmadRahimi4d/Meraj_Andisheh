namespace MerajAndisheh.Core.Domain.Enums;

/// <summary>وضعیت محتوا. حذف واقعی نداریم؛ «بایگانی» محتوا را از سایت پنهان می‌کند.</summary>
public enum ContentStatus : byte
{
    Draft = 1,
    Published = 2,
    Archived = 3
}
