using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Teachers;
using FluentPatcher;
using FluentPatcher.Attributes;

namespace EFlow.Booking.Domain.Subjects;

[PatchFor(typeof(Subject))]
public sealed class SubjectUpdatePatch
{
    public Patchable<string> Name { get; init; }

    public Patchable<TeacherId> TeacherId { get; init; }

    public Patchable<ICollection<GroupId>> GroupIds { get; init; }
}
