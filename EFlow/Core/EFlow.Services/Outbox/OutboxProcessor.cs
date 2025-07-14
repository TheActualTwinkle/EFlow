using System.Text.Json;
using EFlow.Domain;
using EFlow.Domain.Repositories;
using EFlow.Services.Outbox.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EFlow.Services.Outbox;

public class OutboxProcessor(
    IUnitOfWork unitOfWork,
    IPublisher publisher,
    ILogger<OutboxProcessor> logger)
    : IOutboxProcessor
{
    public async Task ProcessPendingAsync(int batchSize, CancellationToken cancellationToken)
    {
        var outboxMessageRepository = unitOfWork.GetRepository<IOutboxMessageRepository>();

        var messages = await outboxMessageRepository.GetUnprocessedAsync(batchSize, cancellationToken);

        foreach (var message in messages)
            try
            {
                var eventType = Type.GetType(message.Type);

                if (eventType is null)
                    throw new InvalidOperationException($"Event type {message.Type} not found");

                var payload = JsonSerializer.Deserialize(message.Payload, eventType);

                if (payload is null)
                    throw new InvalidOperationException($"Payload for {message.Type} cannot be null");

                await publisher.Publish(payload, cancellationToken);

                await outboxMessageRepository.MarkAsProcessedAsync([message.Id], cancellationToken);
            }
            catch (InvalidOperationException e)
            {
                logger.LogError(e, "Error processing outbox message with ID {MessageId}", message.Id);
            }
    }
}