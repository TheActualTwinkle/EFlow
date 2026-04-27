namespace EFlow.Notifications.Application.Email.Models;

public sealed record NotificationMessage
{
    public required string Subject { get; init; }

    public required string Body { get; init; }
    
    public required string RecipientEmail { get; init; }
}
