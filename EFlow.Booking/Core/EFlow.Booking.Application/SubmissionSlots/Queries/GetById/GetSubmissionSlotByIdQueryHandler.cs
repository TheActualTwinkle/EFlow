using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Common.Infrastructure;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Queries;

public class GetSubmissionSlotByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetSubmissionSlotByIdQuery, Result<SubmissionSlotDto>>
{
    public async Task<Result<SubmissionSlotDto>> Handle(GetSubmissionSlotByIdQuery request, CancellationToken cancellationToken)
    {
        var slot = await unitOfWork
            .GetRepository<ISubmissionSlotRepository>()
            .GetByIdAsync(new SubmissionSlotId(request.Id), cancellationToken);

        if (slot is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Submission slot not found")
                    .WithId(request.Id));

        return slot.Adapt<SubmissionSlotDto>();
    }
}