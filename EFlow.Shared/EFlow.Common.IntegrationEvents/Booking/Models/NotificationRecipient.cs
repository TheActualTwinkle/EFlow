using MemoryPack;

namespace EFlow.Common.IntegrationEvents.Booking.Models;

[MemoryPackable]
public sealed partial record NotificationRecipient
{
    public required Guid UserId { get; init; }

    public required string Email { get; init; }
}
