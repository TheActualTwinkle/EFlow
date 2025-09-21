namespace EFlow.Common.Models.Markers;

/// <summary>
///     Marker interface indicating that implementing types are recognized by various services
///     (e.g. <c>OutboxProcessor</c>) as Kafka message.
/// </summary>
public interface IKafkaMessage;