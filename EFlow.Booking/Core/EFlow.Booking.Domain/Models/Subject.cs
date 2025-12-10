namespace EFlow.Booking.Domain.Models;

public sealed class Subject : IEntity
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required Guid TeacherId { get; init; }
    
    public required ICollection<Guid> GroupIds { get; init; }

    public Teacher? Teacher { get; init; }
    
    public ICollection<Group>? Groups { get; init; }
}