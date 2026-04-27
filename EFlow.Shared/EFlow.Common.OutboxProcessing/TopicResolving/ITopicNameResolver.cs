namespace EFlow.Common.OutboxProcessing.TopicResolving;

public interface ITopicNameResolver
{
    public string? ResolveTopicName(string assemblyQualifiedName);
}