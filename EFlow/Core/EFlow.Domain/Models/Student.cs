namespace EFlow.Domain.Models;

public class Student
{
    public required Guid IdentityId { get; init; }

    public required Guid GroupId { get; init; }
    
    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public string? MiddleName { get; init; }

    public required DateOnly BirthDate { get; init; }
    
    public required DateTime CreatedAt { get; init; }

    public Identity? Identity { get; init; }
    
    public Group? Group { get; init; }
}