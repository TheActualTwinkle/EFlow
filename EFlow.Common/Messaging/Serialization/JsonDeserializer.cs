using System.Text.Json;
using Confluent.Kafka;
using MemoryPack;

namespace EFlow.Common.Messaging.Serialization;

public class JsonDeserializer<T> : IDeserializer<T>
{
    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (isNull)
            return default!;

        try
        {
            return MemoryPackSerializer.Deserialize<T>(data) ??
                   throw new InvalidOperationException("Deserialization returned null");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to deserialize message", ex);
        }
    }
}