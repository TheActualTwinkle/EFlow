namespace EFlow.Booking.Domain.Groups;

public sealed record GroupUpdatePatch
{
    public GroupUpdatePatch(string? newName = null) =>
        NewName = newName ??
                  throw new ArgumentException("At least one property must be provided to update group.");

    public string? NewName { get; }
}