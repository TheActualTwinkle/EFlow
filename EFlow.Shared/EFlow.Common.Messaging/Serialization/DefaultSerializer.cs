using Confluent.Kafka;
using MemoryPack;

namespace EFlow.Common.Messaging.Serialization;

public class DefaultSerializer<T> : ISerializer<T>, IDeserializer<T>
{
    public byte[] Serialize(T data, SerializationContext context)
    {
        if (data is byte[] bytes)
            return bytes;

        return MemoryPackSerializer.Serialize(data);
    }

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