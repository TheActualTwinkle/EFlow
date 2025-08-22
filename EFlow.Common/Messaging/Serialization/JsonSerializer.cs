using System.Text.Json;
using Confluent.Kafka;
using MemoryPack;

namespace EFlow.Common.Messaging.Serialization;

public class JsonSerializer<T> : ISerializer<T>
{
    public byte[] Serialize(T data, SerializationContext context)
    {
        if (data is byte[] bytes)
            return bytes;
        
        return MemoryPackSerializer.Serialize(data);
    }
}