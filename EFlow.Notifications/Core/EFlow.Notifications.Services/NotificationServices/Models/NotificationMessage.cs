namespace EFlow.Notifications.Services.NotificationServices;

public sealed record NotificationMessage
{
    public required Guid RecipientUserId { get; init; }

    public string? RecipientEmail { get; init; }

    public required string Subject { get; init; }

    public required string Body { get; init; }
}
