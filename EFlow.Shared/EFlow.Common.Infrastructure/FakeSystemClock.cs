namespace EFlow.Common.Infrastructure;

public sealed class FakeSystemClock(DateTime utcNow) : ISystemClock
{
    public DateTime UtcNow { get; } = utcNow;
}