using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Contracts.SubmissionSlots;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Queries;

public class GetSubmissionSlotByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetSubmissionSlotByIdQuery, Result<SubmissionSlotView>>
{
    public async Task<Result<SubmissionSlotView>> Handle(GetSubmissionSlotByIdQuery request, CancellationToken cancellationToken)
    {
        var slot = await unitOfWork
            .GetQueryService<ISubmissionSlotQueryService>()
            .GetByIdAsync(new SubmissionSlotId(request.Id), cancellationToken);

        if (slot is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Submission slot not found")
                    .WithId(request.Id));

        return Result.Ok(slot);
    }
}