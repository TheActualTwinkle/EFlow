namespace EFlow.Booking.Messaging.TopicResolving;

public interface ITopicNameResolver
{
    public string? ResolveTopicName(string assemblyQualifiedName);
}