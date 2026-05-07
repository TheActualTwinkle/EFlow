using EFlow.Booking.Application.Common.Markers;
using EFlow.Booking.Caching.Interfaces;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Common.Behaviors;

public sealed class CacheableBehavior<TRequest, TResponse>(
    ICacheService cacheService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableRequest
    where TResponse : ResultBase
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
        
        if (response.IsSuccess)
            await cacheService.SetAsync(
                request.CacheKey,
                response,
                request.ExpirationTime,
                cancellationToken);
        
        return response;
    }
}
