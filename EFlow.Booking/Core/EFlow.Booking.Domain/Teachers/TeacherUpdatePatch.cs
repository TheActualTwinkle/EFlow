namespace EFlow.Booking.Domain.Teachers;

public sealed record TeacherUpdatePatch
{
    public TeacherUpdatePatch(
        string? newFirstName = null,
        string? newLastName = null,
        string? newMiddleName = null,
        DateOnly? newBirthDate = null)
    {
        if (newFirstName is null &&
            newLastName is null &&
            newMiddleName is null &&
            newBirthDate is null)
            throw new ArgumentException("At least one property must be provided to update teacher.");

        NewFirstName = newFirstName;
        NewLastName = newLastName;
        NewMiddleName = newMiddleName;
        NewBirthDate = newBirthDate;
    }

    public string? NewFirstName { get; }

    public string? NewLastName { get; }

    public string? NewMiddleName { get; }

    public DateOnly? NewBirthDate { get; }
}