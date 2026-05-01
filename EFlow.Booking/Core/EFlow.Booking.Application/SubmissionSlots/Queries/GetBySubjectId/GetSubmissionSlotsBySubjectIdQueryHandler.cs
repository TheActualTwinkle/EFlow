using EFlow.Booking.Contracts.SubmissionSlots;
using EFlow.Booking.Domain.Subjects;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Queries;

public class GetSubmissionSlotsBySubjectIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetSubmissionSlotsBySubjectIdQuery, Result<IEnumerable<SubmissionSlotView>>>
{
    public async Task<Result<IEnumerable<SubmissionSlotView>>> Handle(GetSubmissionSlotsBySubjectIdQuery request, CancellationToken cancellationToken)
    {
        var slots = await unitOfWork
            .GetQueryService<ISubmissionSlotQueryService>()
            .GetBySubjectIdAsync(new SubjectId(request.SubjectId), cancellationToken);

        return Result.Ok(slots);
    }
}