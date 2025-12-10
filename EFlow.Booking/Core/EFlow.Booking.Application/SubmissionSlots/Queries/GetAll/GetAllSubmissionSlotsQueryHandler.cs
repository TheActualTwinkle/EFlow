using EFlow.Booking.Domain;
using EFlow.Booking.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Queries;

public class GetAllSubmissionSlotsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllSubmissionSlotsQuery, Result<IEnumerable<SubmissionSlotDto>>>
{
    public async Task<Result<IEnumerable<SubmissionSlotDto>>> Handle(GetAllSubmissionSlotsQuery request, CancellationToken cancellationToken)
    {
        var slots = (await unitOfWork
                .GetRepository<ISubmissionSlotRepository>()
                .GetAllAsync(cancellationToken))
            .Adapt<IEnumerable<SubmissionSlotDto>>();

        return Result.Ok(slots);
    }
}