namespace EFlow.Booking.WebApi.Contracts.Subjects;

public record UpdateSubjectRequest
{
    public string? Name { get; init; }

    public Guid? TeacherId { get; init; }
    
    public IEnumerable<Guid>? GroupIds { get; init; }
}
