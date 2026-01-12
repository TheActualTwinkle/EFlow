using EFlow.Booking.Domain;
using EFlow.Booking.Domain.Models;
using EFlow.Booking.Domain.Repositories;
using EFlow.Booking.IntegrationEvents;
using FluentResults;
using MediatR;
using MemoryPack;

namespace EFlow.Booking.Application.SubmissionSlots.Commands;

public class CreateSubmissionSlotCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateSubmissionSlotCommand, Result<Guid>>
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

        var createdEvent = new SubmissionSlotCreatedIntegrationEvent
        {
            Id = slot.Id,
            SubjectId = slot.SubjectId,
            StartTime = slot.StartTime,
            EndTime = slot.EndTime,
            MaxStudents = slot.MaxStudents,
            Location = slot.Location
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