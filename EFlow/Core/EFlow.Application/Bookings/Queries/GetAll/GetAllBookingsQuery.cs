using FluentResults;
using MediatR;

namespace EFlow.Application.Bookings.Queries;

public record GetAllBookingsQuery : IRequest<Result<IEnumerable<BookingDto>>>;