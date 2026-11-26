using MerajAndisheh.Core.Abstractions;

namespace MerajAndisheh.Tests;

/// <summary>ساعت دست‌ساز برای تست قواعد زمانی (انقضای ۲ دقیقه، ۶۰ ثانیه، یک ساعت).</summary>
public sealed class FakeClock(DateTime startUtc) : IClock
{
    public DateTime UtcNow { get; private set; } = startUtc;

    public void Advance(TimeSpan by) => UtcNow += by;
}
