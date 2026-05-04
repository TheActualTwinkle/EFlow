using FluentPatcher;

namespace EFlow.Booking.WebApi.Contracts.Groups;

public record UpdateGroupRequest
{
    public Patchable<string> Name { get; init; }
}
