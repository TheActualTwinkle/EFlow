using EFlow.Booking.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Admins.Commands;

public record DeleteAdminCommand : IRequest<Result>, ITransactionalRequest
{
    public required Guid Id { get; init; }
}