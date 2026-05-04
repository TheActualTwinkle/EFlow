using FluentPatcher;
using FluentPatcher.Attributes;

namespace EFlow.Booking.Domain.Teachers;

[PatchFor(typeof(Teacher))]
public sealed class TeacherUpdatePatch
{
    public Patchable<string> FirstName { get; init; }

    public Patchable<string> LastName { get; init; }

    public Patchable<string?> MiddleName { get; init; }

    public Patchable<DateOnly> BirthDate { get; init; }
}
