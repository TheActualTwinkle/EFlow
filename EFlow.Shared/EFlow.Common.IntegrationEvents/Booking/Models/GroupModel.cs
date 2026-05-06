using MemoryPack;

namespace EFlow.Common.IntegrationEvents.Booking.Models;

[MemoryPackable]
public sealed partial record GroupModel
{
    public required Guid Id { get; init; }
    
    public required string Name { get; init; }
}