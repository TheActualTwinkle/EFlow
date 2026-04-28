using EFlow.Booking.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Teachers.Commands;

public record DeleteTeacherCommand : IRequest<Result>, ITransactionalRequest
{
    public required Guid Id { get; init; }
}