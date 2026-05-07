using EFlow.Booking.Application.Common.Markers;
using EFlow.Booking.Caching.Interfaces;
using EFlow.Booking.Caching.Settings;
using MediatR;
using Microsoft.Extensions.Options;

namespace EFlow.Booking.Application.Common.Behaviors;

public sealed class CacheableBehavior<TRequest, TResponse>(
    ICacheService cacheService,
    IOptions<CacheSettings> cacheSettings)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableRequest
    where TResponse : class
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = new())
    {
        var cachedResponse = await cacheService.GetAsync<TResponse>(request.CacheKey, cancellationToken);

        if (cachedResponse is not null)
            return cachedResponse;
        
        var response = await next(cancellationToken);
        
        await cacheService.SetAsync(
            request.CacheKey,
            response,
            cacheSettings.Value.DefaultExpirationTime,
            cancellationToken);
        
        return response;
    }
}