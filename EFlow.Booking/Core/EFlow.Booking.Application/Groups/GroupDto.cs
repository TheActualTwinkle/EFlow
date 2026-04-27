using EFlow.Booking.Application.Students;

namespace EFlow.Booking.Application.Groups;

public record GroupDto
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required ICollection<StudentDto> Students { get; init; }
}