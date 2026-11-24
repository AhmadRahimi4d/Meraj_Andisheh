using MerajAndisheh.Core.Abstractions;
using MerajAndisheh.Infrastructure.Time;
using Xunit;

namespace MerajAndisheh.Tests;

public class SystemClockTests
{
    [Fact]
    public void UtcNow_ReturnsUtcTimeCloseToSystemTime()
    {
        IClock clock = new SystemClock();

        var before = DateTime.UtcNow;
        var now = clock.UtcNow;
        var after = DateTime.UtcNow;

        Assert.Equal(DateTimeKind.Utc, now.Kind);
        Assert.InRange(now, before, after);
    }
}
