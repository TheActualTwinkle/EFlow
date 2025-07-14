using EFlow.Services.Outbox;
using EFlow.Services.Outbox.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EFlow.Services.Background;

public class OutboxProcessorBackgroundService(
    IServiceScopeFactory scopeFactory,
    OutboxProcessorSettings settings,
    ILogger<OutboxProcessorBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Outbox Processor started");

        await using var scope = scopeFactory.CreateAsyncScope();

        var outboxProcessor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await outboxProcessor.ProcessPendingAsync(settings.BatchSize, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing outbox messages");
            }

            logger.LogInformation(
                "Outbox Processor completed a cycle, waiting for the next one in {Delay} seconds",
                settings.ProcessInterval);

            await Task.Delay(settings.ProcessInterval, cancellationToken);
        }
    }
}