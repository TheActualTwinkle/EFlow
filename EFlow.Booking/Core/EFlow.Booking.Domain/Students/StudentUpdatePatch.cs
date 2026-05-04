using FluentPatcher;
using FluentPatcher.Attributes;

namespace EFlow.Booking.Domain.Students;

[PatchFor(typeof(Student))]
public class StudentUpdatePatch
{
    public Patchable<string> FirstName { get; init; }

    public Patchable<string> LastName { get; init; }

    public Patchable<string?> MiddleName { get; init; }

    public Patchable<DateOnly> BirthDate { get; init; }
}
