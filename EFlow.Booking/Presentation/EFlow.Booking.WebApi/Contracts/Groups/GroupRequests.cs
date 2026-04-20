namespace EFlow.Booking.WebApi.Contracts.Groups;

public record CreateGroupRequest
{
    public required string Name { get; init; }
}
