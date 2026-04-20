using EFlow.Booking.Messaging.Outbox.Interfaces;
using EFlow.Booking.Messaging.Outbox.MessageProcessing.Factories.Interfaces;
using EFlow.Common.Domain.Repositories;
using EFlow.Common.Infrastructure;
using Microsoft.Extensions.Logging;

namespace EFlow.Booking.Messaging.Outbox;

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
                var resolvedMessageType = Type.GetType(message.Type);

                if (resolvedMessageType is null)
                {
                    await HandleCorruptedMessage(
                        outboxMessageRepository,
                        message.Id, 
                        $"Unable to resolve outbox message type {message.Type}. Will skip and mark message as processed",
                        processedMessageIds,
                        cancellationToken);

                    continue;
                }

                var messageProcessor = messageProcessorFactory.Get(resolvedMessageType);

                if (messageProcessor is null)
                {
                    await HandleCorruptedMessage(
                        outboxMessageRepository,
                        message.Id,
                        $"No message processor found for type {resolvedMessageType.FullName}. Will skip and mark message as processed.",
                        processedMessageIds,
                        cancellationToken);

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

        logger.LogInformation("Processed {MessageCount} outbox messages", processedMessageIds.Count);
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

    private async Task HandleCorruptedMessage(
        IOutboxMessageRepository outboxMessageRepository,
        Guid messageId,
        string error,
        List<Guid> processedMessageIds,
        CancellationToken cancellationToken)
    {
        logger.LogError(error);

        await outboxMessageRepository.AddErrorAsync(messageId, error, cancellationToken);

        processedMessageIds.Add(messageId);
    }
}