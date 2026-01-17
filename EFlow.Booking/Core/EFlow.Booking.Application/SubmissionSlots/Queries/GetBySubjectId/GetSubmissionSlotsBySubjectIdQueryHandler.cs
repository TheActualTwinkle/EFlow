using EFlow.Common.Domain;
using EFlow.Common.Infrastructure;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Queries;

public class GetSubmissionSlotsBySubjectIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetSubmissionSlotsBySubjectIdQuery, Result<IEnumerable<SubmissionSlotDto>>>
{
    public async Task<Result<IEnumerable<SubmissionSlotDto>>> Handle(GetSubmissionSlotsBySubjectIdQuery request, CancellationToken cancellationToken)
    {
        var slots = (await unitOfWork
                .GetRepository<ISubmissionSlotRepository>()
                .GetBySubjectIdAsync(request.SubjectId, cancellationToken))
            .Adapt<IEnumerable<SubmissionSlotDto>>();

        return Result.Ok(slots);
    }
}