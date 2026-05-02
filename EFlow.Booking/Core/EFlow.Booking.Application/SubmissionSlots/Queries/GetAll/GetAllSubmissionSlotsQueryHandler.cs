using EFlow.Booking.Contracts.SubmissionSlots;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Queries;

public class GetAllSubmissionSlotsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllSubmissionSlotsQuery, Result<IEnumerable<SubmissionSlotView>>>
{
    public async Task<Result<IEnumerable<SubmissionSlotView>>> Handle(GetAllSubmissionSlotsQuery request, CancellationToken cancellationToken)
    {
        var slots = await unitOfWork
            .GetQueryService<ISubmissionSlotQueryService>()
            .GetAllAsync(cancellationToken);

        return Result.Ok(slots);
    }
}