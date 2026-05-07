using EFlow.Booking.Caching.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace EFlow.Booking.Caching;

public sealed class InMemoryCacheService(IMemoryCache memoryCache) : ICacheService
{
    public ValueTask<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = new()) where T : class =>
        ValueTask.FromResult(memoryCache.Get<T>(key));

    public ValueTask SetAsync<T>(
        string key,
        T value,
        TimeSpan expirationTime,
        CancellationToken cancellationToken = new()) where T : class
    {
        memoryCache.Set(
            key,
            value,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expirationTime
            });

        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveAsync(
        string key,
        CancellationToken cancellationToken = new())
    {
        memoryCache.Remove(key);

        return ValueTask.CompletedTask;
    }
}
