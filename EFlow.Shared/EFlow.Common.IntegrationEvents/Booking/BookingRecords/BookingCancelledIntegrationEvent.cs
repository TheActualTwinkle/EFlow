using EFlow.Common.IntegrationEvents.Booking.Models;
using EFlow.Common.Markers;
using MemoryPack;

namespace EFlow.Common.IntegrationEvents.Booking.BookingRecords;

[MemoryPackable]
public partial record BookingCancelledIntegrationEvent : IKafkaMessage
{
    public required BookingRecordModel BookingRecord { get; init; }

    public required IEnumerable<NotificationRecipient> NotificationRecipients { get; init; }
}
