using EFlow.Domain;
using EFlow.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Application.Bookings.Queries;

public class GetBookingsByStudentIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetBookingsByStudentIdQuery, Result<IEnumerable<BookingDto>>>
{
    public async Task<Result<IEnumerable<BookingDto>>> Handle(GetBookingsByStudentIdQuery request, CancellationToken cancellationToken)
    {
        var bookings = (await unitOfWork
                .GetRepository<IBookingRepository>()
                .GetByStudentIdAsync(request.StudentId, cancellationToken))
            .Adapt<IEnumerable<BookingDto>>();

        return Result.Ok(bookings);
    }
}