using EFlow.Booking.Caching.Interfaces;

namespace EFlow.Booking.Caching;

public sealed class CacheService : ICacheService
{
    public Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default) where T : class
    {
        throw new NotImplementedException();
    }

    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expirationTime,
        CancellationToken cancellationToken = default) where T : class
    {
        throw new NotImplementedException();
    }
}