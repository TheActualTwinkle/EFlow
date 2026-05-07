namespace EFlow.Booking.Application.Common.Markers;

public interface ICacheableRequest
{
    public string CacheKey { get; }
}