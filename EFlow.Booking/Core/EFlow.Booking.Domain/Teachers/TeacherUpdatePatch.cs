using FluentPatcher;
using FluentPatcher.Attributes;

namespace EFlow.Booking.Domain.Teachers;

[PatchFor(typeof(Teacher))]
public sealed record TeacherUpdatePatch
{
    public Patchable<string> FirstName { get; }

    public Patchable<string> LastName { get; }

    public Patchable<string?> MiddleName { get; }

    public Patchable<DateOnly> BirthDate { get; }
}