namespace EFlow.Common.Markers;

/// <summary>
///     Marker interface indicating that implementing types are recognized by various services
///     (e.g. <c>OutboxProcessor</c>) as Kafka message and must be treated as such.
/// </summary>
public interface IKafkaMessage;