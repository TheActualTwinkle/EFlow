using EFlow.Booking.Contracts.SubmissionSlots;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Queries;

public class GetAvailableSubmissionSlotsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAvailableSubmissionSlotsQuery, Result<IEnumerable<SubmissionSlotView>>>
{
    public async Task<Result<IEnumerable<SubmissionSlotView>>> Handle(GetAvailableSubmissionSlotsQuery request, CancellationToken cancellationToken)
    {
        var slots = await unitOfWork
            .GetQueryService<ISubmissionSlotQueryService>()
            .GetAvailableSlotsAsync(request.FromDate, cancellationToken);

        return Result.Ok(slots);
    }
}