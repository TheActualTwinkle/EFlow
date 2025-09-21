using EFlow.Common.Models.SubmissionSlot;
using EFlow.Domain;
using EFlow.Domain.Models;
using EFlow.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;
using MemoryPack;

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
            AllowAllGroups = request.AllowAllGroups,
            AllowedGroupIds = request.AllowedGroupIds,
            Location = request.Location
        };

        await unitOfWork
            .GetRepository<ISubmissionSlotRepository>()
            .CreateAsync(slot, cancellationToken);

        var createdEvent = new SubmissionSlotCreatedMessage
        {
            SubmissionSlot = slot.Adapt<SubmissionSlotModel>()
        };

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = createdEvent.GetType().AssemblyQualifiedName ??
                   throw new InvalidOperationException("Event type cannot be null"),
            Payload = MemoryPackSerializer.Serialize(createdEvent),
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork
            .GetRepository<IOutboxMessageRepository>()
            .CreateAsync(outboxMessage, cancellationToken);

        return Result.Ok(slot.Id);
    }
}