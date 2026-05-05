using FluentPatcher;
using FluentPatcher.Attributes;

namespace EFlow.Booking.Domain.Groups;

[PatchFor(typeof(Group))]
public sealed class GroupUpdatePatch
{
    public Patchable<string> Name { get; init; }
}
