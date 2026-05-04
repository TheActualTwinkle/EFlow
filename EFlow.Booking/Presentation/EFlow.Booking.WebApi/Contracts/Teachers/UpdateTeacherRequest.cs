using FluentPatcher;

namespace EFlow.Booking.WebApi.Contracts.Teachers;

public record UpdateTeacherRequest
{
    public Patchable<string> FirstName { get; init; }

    public Patchable<string> LastName { get; init; }

    public Patchable<string?> MiddleName { get; init; }

    public Patchable<DateOnly> BirthDate { get; init; }
}
