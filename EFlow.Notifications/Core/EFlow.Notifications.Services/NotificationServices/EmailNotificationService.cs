using EFlow.Notifications.Services.NotificationServices.Interfaces;
using Microsoft.Extensions.Logging;

namespace EFlow.Notifications.Services.NotificationServices;

public class EmailNotificationService(ILogger<EmailNotificationService> logger) : IEmailNotificationService
{
    public Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = new())
    {
        // TODO
        logger.LogInformation(
            "Email notification to {RecipientEmail} ({RecipientUserId}). Subject: {Subject}. Body: {Body}",
            message.RecipientEmail ?? "unknown-email",
            message.RecipientUserId,
            message.Subject,
            message.Body);

        return Task.CompletedTask;
    }
}
