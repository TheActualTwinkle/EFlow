using EFlow.Booking.Contracts.Students;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Queries.GetNotBookedStudents;

public sealed record GetNotBookedStudentsQuery : IRequest<Result<NotBookedStudentsView>>
{
    public required Guid SlotId { get; init; }
}