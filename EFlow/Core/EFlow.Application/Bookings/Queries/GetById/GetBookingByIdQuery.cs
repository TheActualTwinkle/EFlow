using FluentResults;
using MediatR;

namespace EFlow.Application.Bookings.Queries;

public record GetBookingByIdQuery : IRequest<Result<BookingDto>>
{
    public required Guid Id { get; init; }
}