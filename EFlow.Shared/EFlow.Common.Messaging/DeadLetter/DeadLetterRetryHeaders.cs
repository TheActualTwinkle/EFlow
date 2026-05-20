using System.Text;
using Confluent.Kafka;

namespace EFlow.Common.Messaging.DeadLetter;

public static class DeadLetterRetryHeaders
{
    private const string TargetConsumerGroupHeaderName = "eflow-dlq-target-consumer-group";

    private const string AttemptHeaderName = "eflow-dlq-attempt";

    public static Headers CreateRetryHeaders(string targetConsumerGroup, int attempt)
    {
        var headers = new Headers
        {
            { TargetConsumerGroupHeaderName, Encoding.UTF8.GetBytes(targetConsumerGroup) },
            { AttemptHeaderName, Encoding.UTF8.GetBytes(attempt.ToString()) }
        };

        return headers;
    }

    public static string? GetTargetConsumerGroup(Headers? headers) =>
        GetString(headers, TargetConsumerGroupHeaderName);

    public static int GetAttempt(Headers? headers) =>
        int.TryParse(GetString(headers, AttemptHeaderName), out var attempt) ? attempt : 0;

    private static string? GetString(Headers? headers, string key)
    {
        if (headers is null)
            return null;

        try
        {
            var value = headers.GetLastBytes(key);

            return value is null ? null : Encoding.UTF8.GetString(value);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }
}