using FluentResults;
using MediatR;

namespace EFlow.Application.Bookings.Queries;

public record GetBookingsByStudentIdQuery : IRequest<Result<IEnumerable<BookingDto>>>
{
    public required Guid StudentId { get; init; }
}