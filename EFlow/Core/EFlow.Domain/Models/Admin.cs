namespace EFlow.Domain.Models;

public sealed class Admin : IEntity
{
    public Guid Id =>
        IdentityId;

    public required Guid IdentityId { get; init; }

    public required DateTime CreatedAt { get; init; }

    public Identity? Identity { get; init; }
}