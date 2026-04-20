namespace EFlow.Booking.WebApi.Contracts.SubmissionSlots;

public record GetAvailableSubmissionSlotsRequest
{
    public required DateTime FromDate { get; init; }
}
