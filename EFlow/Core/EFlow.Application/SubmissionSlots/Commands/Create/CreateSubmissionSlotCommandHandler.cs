using System.Text.Json;
using EFlow.Application.SubmissionSlots.Commands.Events;
using EFlow.Domain;
using EFlow.Domain.Models;
using EFlow.Domain.Repositories;
using FluentResults;
using Mapster;
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
        
        var createdEvent = new SubmissionSlotCreatedEvent
        {
            SubmissionSlot = slot.Adapt<SubmissionSlotDto>()
        };

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = createdEvent.GetType().AssemblyQualifiedName ??
                   throw new InvalidOperationException("Event type cannot be null"),
            Payload = JsonSerializer.Serialize(createdEvent),
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork
            .GetRepository<IOutboxMessageRepository>()
            .CreateAsync(outboxMessage, cancellationToken);

        return Result.Ok(slot.Id);
    }
}