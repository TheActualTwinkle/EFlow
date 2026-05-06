using EFlow.Booking.Contracts.SubmissionSlots;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Queries;

public sealed class GetSubmissionSlotReminderSnapshotQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetSubmissionSlotReminderSnapshotQuery, Result<IEnumerable<SubmissionSlotReminderSnapshotView>>>
{
    public async Task<Result<IEnumerable<SubmissionSlotReminderSnapshotView>>> Handle(
        GetSubmissionSlotReminderSnapshotQuery request,
        CancellationToken cancellationToken)
    {
        var snapshots = await unitOfWork
            .GetQueryService<ISubmissionSlotQueryService>()
            .GetReminderSnapshotAsync(cancellationToken);

        return Result.Ok(snapshots);
    }
}
