using EFlow.Application.Common.Errors.Abstractions;
using EFlow.Domain;
using EFlow.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Application.SubmissionSlots.Queries;

public class GetSubmissionSlotByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetSubmissionSlotByIdQuery, Result<SubmissionSlotDto>>
{
    public async Task<Result<SubmissionSlotDto>> Handle(GetSubmissionSlotByIdQuery request, CancellationToken cancellationToken)
    {
        var slot = await unitOfWork
            .GetRepository<ISubmissionSlotRepository>()
            .GetByIdAsync(request.Id, cancellationToken);

        if (slot is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Submission slot not found")
                    .WithId(request.Id));

        return slot.Adapt<SubmissionSlotDto>();
    }
}