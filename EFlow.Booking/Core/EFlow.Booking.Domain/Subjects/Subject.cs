using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Teachers;
using EFlow.Common.Domain;

namespace EFlow.Booking.Subjects;

public sealed class Subject : Entity
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required Guid TeacherId { get; init; }
    
    public required ICollection<Guid> GroupIds { get; init; }

    public Teacher? Teacher { get; init; }
    
    public ICollection<Group>? Groups { get; init; }
}