namespace EFlow.Common.Infrastructure;

public interface ISystemClock
{
    DateTime UtcNow { get; }
}
