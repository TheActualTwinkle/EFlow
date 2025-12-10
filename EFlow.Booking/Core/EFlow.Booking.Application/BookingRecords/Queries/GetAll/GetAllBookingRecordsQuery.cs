using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Queries;

public record GetAllBookingRecordsQuery : IRequest<Result<IEnumerable<BookingRecordDto>>>;