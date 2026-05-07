namespace EFlow.Booking.Application.Common.Markers;

public interface ICacheableRequest
{
    public string CacheKey { get; }
    
    public TimeSpan ExpirationTime { get; }
}