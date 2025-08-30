using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace EFlow.Common.Messaging.Consumers;

public class CommitLogConsumer<TKey, TValue> : ICommitLogConsumer<TKey, TValue>
{
    private readonly IConsumer<TKey, TValue> _consumer;

    private readonly ILogger<CommitLogConsumer<TKey, TValue>> _logger;

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
            .SetErrorHandler((_, e) => logger.LogError("Kafka Error: {reason}", e.Reason))
            .SetLogHandler((_, m) => logger.LogInformation("Kafka Log: {message}", m.Message))
            .Build();
    }

    public IObservable<TValue> FromTopic(string topic) =>
        Observable.Create<TValue>(observer =>
        {
            var cts = new CancellationTokenSource();

            NewThreadScheduler.Default.ScheduleLongRunning(_ =>
            {
                _logger.LogInformation("Starting consumer for topic {Topic}", topic);

                _consumer.Subscribe(topic);

                try
                {
                    while (!cts.IsCancellationRequested)
                        try
                        {
                            var result = _consumer.Consume(cts.Token);

                            if (result?.Message == null)
                                continue;

                            _consumer.Commit(result);

                            observer.OnNext(result.Message.Value);
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

                    observer.OnCompleted();

                    _logger.LogInformation("Consumer for topic {Topic} closed", topic);
                }
            });

            return () => cts.Cancel();
        });
}