using EFlow.Booking.Contracts.SubmissionSlots;
using EFlow.Booking.Domain.Teachers;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Queries.GetByTeacherId;

public sealed class GetSubmissionSlotsByTeacherIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetSubmissionSlotsByTeacherIdQuery, Result<IEnumerable<SubmissionSlotView>>>
{
    public async Task<Result<IEnumerable<SubmissionSlotView>>> Handle(GetSubmissionSlotsByTeacherIdQuery request, CancellationToken cancellationToken)
    {
        var slots = await unitOfWork
            .GetQueryService<ISubmissionSlotQueryService>()
            .GetByTeacherIdAsync(new TeacherId(request.TeacherId), cancellationToken);

        return Result.Ok(slots);
    }
}