using System.Collections.Concurrent;
using EFlow.Booking.Caching.Interfaces;

namespace EFlow.Booking.Caching;

public sealed class InMemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, object> _collection = new();

    public ValueTask<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = new()) where T : class =>
        ValueTask.FromResult(_collection.GetValueOrDefault(key) as T);

    public ValueTask SetAsync<T>(
        string key,
        T value,
        TimeSpan expirationTime,
        CancellationToken cancellationToken = new()) where T : class
    {
        _collection.AddOrUpdate(key, value, (_, _) => value);

        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveAsync(
        string key,
        CancellationToken cancellationToken = new())
    {
        _collection.TryRemove(key, out _);

        return ValueTask.CompletedTask;
    }
}