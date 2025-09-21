using EFlow.Domain;
using EFlow.Domain.Repositories;
using EFlow.Messaging.Outbox.Interfaces;
using EFlow.Messaging.Outbox.MessageProcessing.Factories.Interfaces;
using Microsoft.Extensions.Logging;

namespace EFlow.Messaging.Outbox;

public class OutboxProcessor(
    IUnitOfWorkFactory unitOfWorkFactory,
    IOutboxMessageProcessorFactory messageProcessorFactory,
    ILogger<OutboxProcessor> logger)
    : IOutboxProcessor
{
    public async Task ProcessPendingAsync(int batchSize, CancellationToken cancellationToken = new())
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateTransactionalAsync(cancellationToken: cancellationToken);

        var outboxMessageRepository = unitOfWork.GetRepository<IOutboxMessageRepository>();

        var messages = await outboxMessageRepository.GetUnprocessedAsync(batchSize, cancellationToken);

        if (messages.Count == 0)
            return;

        var processedMessageIds = new List<Guid>();
        
        foreach (var message in messages)
            try
            {
                var messageType = Type.GetType(message.Type);

                if (messageType is null)
                    throw new InvalidOperationException($"Outbox message type {message.Type} is null");

                var messageProcessor = messageProcessorFactory.Get(messageType);

                if (messageProcessor is null)
                {
                    logger.LogError(
                        "No message processor found for type {MessageType}. Will skip and mark as processed.",
                        messageType.FullName);

                    continue;
                }

                await messageProcessor.ProcessAsync(message, cancellationToken);
                
                processedMessageIds.Add(message.Id);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error processing outbox message with ID {MessageId}", message.Id);
                
                await outboxMessageRepository.AddErrorAsync(message.Id, e.ToString(), cancellationToken);
            }

        if (processedMessageIds.Count != 0)
            await outboxMessageRepository.MarkAsProcessedAsync(processedMessageIds, cancellationToken);

        await unitOfWork.CommitTransactionAsync(cancellationToken);

        logger.LogInformation("Processed {MessageCount} outbox messages", messages.Count);
    }

    public async Task DeleteProcessedAsync(TimeSpan deleteAfter, CancellationToken cancellationToken = new())
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateTransactionalAsync(cancellationToken: cancellationToken);

        var beforeDate = DateTime.UtcNow - deleteAfter;

        logger.LogInformation("Deleting processed outbox messages older than {BeforeDate}", beforeDate);

        await unitOfWork
            .GetRepository<IOutboxMessageRepository>()
            .DeleteProcessedAsync(beforeDate, cancellationToken);

        await unitOfWork.CommitTransactionAsync(cancellationToken);

        logger.LogInformation("Deleted outbox messages older than {BeforeDate}", beforeDate);
    }
}