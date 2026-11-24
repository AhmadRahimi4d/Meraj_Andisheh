using MerajAndisheh.Core.Abstractions;

namespace MerajAndisheh.Infrastructure.Time;

/// <summary>پیاده‌سازی IClock با ساعت سیستم.</summary>
public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
