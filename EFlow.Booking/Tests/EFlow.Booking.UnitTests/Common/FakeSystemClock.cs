using EFlow.Common.Infrastructure;

namespace EFlow.Booking.UnitTests.Common;

public sealed class FakeSystemClock(DateTime utcNow) : ISystemClock
{
    public DateTime UtcNow { get; } = utcNow;
}
