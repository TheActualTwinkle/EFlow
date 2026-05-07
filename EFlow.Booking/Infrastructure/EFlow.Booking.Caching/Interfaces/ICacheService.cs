namespace EFlow.Booking.Caching.Interfaces;

public interface ICacheService
{
    public ValueTask<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default) where T : class;
    
    public ValueTask SetAsync<T>(
        string key,
        T value,
        TimeSpan expirationTime,
        CancellationToken cancellationToken = default) where T : class;
}