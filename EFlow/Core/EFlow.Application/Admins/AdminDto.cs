namespace EFlow.Application.Admins;

public record AdminDto
{
    public required Guid IdentityId { get; init; }
    
    public required string UserName { get; init; }

    public required DateTime CreatedAt { get; init; }
}