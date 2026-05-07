using EFlow.Booking.Caching.Constants;
using EFlow.Booking.Caching.Interfaces;
using MediatR;

namespace EFlow.Booking.Application.Groups.Notifications;

public sealed class CacheInvalidationGroupNotificationHandler(ICacheService cacheService) : INotificationHandler<CacheInvalidationGroupNotification> 
{
    public async Task Handle(CacheInvalidationGroupNotification notification, CancellationToken cancellationToken) =>
        await cacheService.RemoveAsync(CacheKeys.Groups.All, cancellationToken);
}