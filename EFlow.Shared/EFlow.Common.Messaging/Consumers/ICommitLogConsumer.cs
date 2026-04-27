namespace EFlow.Common.Messaging.Consumers;

public interface ICommitLogConsumer<TKey, out TValue>
{
    public IObservable<TValue> FromTopic(string topic);
}