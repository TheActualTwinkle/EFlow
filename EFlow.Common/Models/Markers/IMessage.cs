namespace EFlow.Common.Models.Markers;

/// <summary>
/// Marker interface indicating that implementing types are recognized by various services
/// (e.g. <c>OutboxProcessor</c>) for special handling, such as publishing to a message broker like Kafka.
/// </summary>
public interface IMessage;