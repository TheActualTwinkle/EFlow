using EFlow.Domain;
using EFlow.Domain.Models;
using EFlow.Domain.Repositories;
using FluentResults;
using MediatR;

namespace EFlow.Application.SubmissionSlots.Commands;

public class CreateSubmissionSlotCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateSubmissionSlotCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateSubmissionSlotCommand request, CancellationToken cancellationToken)
    {
        var slot = new SubmissionSlot
        {
            Id = Guid.NewGuid(),
            SubjectId = request.SubjectId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            MaxStudents = request.MaxStudents,
            Location = request.Location
        };

        await unitOfWork
            .GetRepository<ISubmissionSlotRepository>()
            .CreateAsync(slot, cancellationToken);

        return Result.Ok(slot.Id);
    }
}