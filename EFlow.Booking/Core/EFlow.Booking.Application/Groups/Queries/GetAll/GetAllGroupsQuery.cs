using EFlow.Booking.Application.Common.Markers;
using EFlow.Booking.Caching.Constants;
using EFlow.Booking.Contracts.Groups;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Groups.Queries;

public record GetAllGroupsQuery : IRequest<Result<IEnumerable<GroupView>>>, ICacheableRequest
{
    public string CacheKey => 
        CacheKeys.Groups.All;

    public TimeSpan ExpirationTime =>
        TimeSpan.FromMinutes(5);
}