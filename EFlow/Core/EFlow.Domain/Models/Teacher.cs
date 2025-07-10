namespace EFlow.Domain.Models;

public sealed class Teacher
{
    public required Guid IdentityId { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public string? MiddleName { get; init; }

    public required DateOnly BirthDate { get; init; }
    
    public DateTime? CreatedAt { get; init; }

    public Identity? Identity { get; init; }
}