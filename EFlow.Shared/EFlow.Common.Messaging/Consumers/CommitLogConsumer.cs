using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace EFlow.Common.Messaging.Consumers;

public class CommitLogConsumer<TKey, TValue> : ICommitLogConsumer<TKey, TValue>
{
    private readonly IConsumer<TKey, TValue> _consumer;

    private readonly ILogger<CommitLogConsumer<TKey, TValue>> _logger;

    private Task? _consumingTask;
    
    private CancellationTokenSource? _stoppingCts;

    public CommitLogConsumer(
        ConsumerConfig consumerConfig,
        IDeserializer<TKey> keyDeserializer,
        IDeserializer<TValue> valueDeserializer,
        ILogger<CommitLogConsumer<TKey, TValue>> logger)
    {
        _logger = logger;

        _consumer = new ConsumerBuilder<TKey, TValue>(consumerConfig)
            .SetKeyDeserializer(keyDeserializer)
            .SetValueDeserializer(valueDeserializer)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka Error: {reason}", e.Reason))
            .Build();
    }

    public Task StartAsync(
        string topic,
        Func<TValue, CancellationToken, ValueTask<bool>> handler,
        CancellationToken cancellationToken = new())
    {
        if (_consumingTask is not null)
            throw new InvalidOperationException("Commit log consumer is already running.");

        _stoppingCts?.Dispose();
        
        var stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        _stoppingCts = stoppingCts;
        _consumingTask = Task.Run(() => Consume(topic, handler, stoppingCts.Token), CancellationToken.None);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = new())
    {
        if (_consumingTask is null)
            return;

        try
        {
            if (_stoppingCts is not null)
                await _stoppingCts.CancelAsync();
            
            await _consumingTask.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Stopping consumer was canceled.");
        }
        finally
        {
            if (_consumingTask.IsCompleted)
            {
                _consumingTask = null;
                _stoppingCts?.Dispose();
                _stoppingCts = null;
            }
        }
    }

    private async Task Consume(string topic, Func<TValue, CancellationToken, ValueTask<bool>> handler, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting consumer for topic {Topic}", topic);

        _consumer.Subscribe(topic);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
                try
                {
                    var result = _consumer.Consume(cancellationToken);

                    if (result?.Message is null)
                    {
                        // TODO: Send to DQL as non-retryable
                        
                        continue;
                    }

                    try
                    {
                        var isHandledSuccessfully = await handler(result.Message.Value, cancellationToken);

                        if (isHandledSuccessfully)
                        {
                            _consumer.Commit(result);

                            _logger.LogDebug("Message \"{Type}\" was handled successfully and committed.", result.Message.Value?.GetType().Name);
                        }
                        else
                        {
                            _logger.LogWarning("Message \"{Type}\" was not handled successfully.", result.Message.Value?.GetType().Name);
                            // TODO: Send to DQL as retryable
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Handler failed for topic: \"{Topic}\". Exception: {Exception}", topic, ex.Message);
                        // TODO: Send to DQL as retryable
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Consumer operation was canceled.");

                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error consuming message from Kafka topic {Topic}", topic);
                }
        }
        finally
        {
            _consumer.Close();

            _logger.LogInformation("Consumer for topic {Topic} closed", topic);
        }
    }
}
