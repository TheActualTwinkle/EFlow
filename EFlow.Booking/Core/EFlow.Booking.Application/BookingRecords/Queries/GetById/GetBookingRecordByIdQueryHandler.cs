using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Common.Domain;
using EFlow.Common.Infrastructure;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Queries;

public class GetBookingRecordByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetBookingRecordByIdQuery, Result<BookingRecordDto>>
{
    public async Task<Result<BookingRecordDto>> Handle(GetBookingRecordByIdQuery request, CancellationToken cancellationToken)
    {
        var booking = await unitOfWork
            .GetRepository<IBookingRecordRepository>()
            .GetByIdAsync(request.Id, cancellationToken);

        if (booking is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("BookingRecord not found")
                    .WithId(request.Id));

        return booking.Adapt<BookingRecordDto>();
    }
}