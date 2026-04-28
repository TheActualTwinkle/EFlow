using MemoryPack;

namespace EFlow.Common.IntegrationEvents.Booking.Models;

[MemoryPackable]
public sealed partial record BookingRecordModel
{
    public required Guid Id { get; init; }
    
    public required string StudentFullName { get; init; }
    
    public required SubmissionSlotModel SubmissionSlotModel { get; init; }
}