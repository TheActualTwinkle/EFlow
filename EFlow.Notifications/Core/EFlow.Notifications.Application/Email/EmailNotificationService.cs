using EFlow.Notifications.Application.Email.Interfaces;
using EFlow.Notifications.Application.Email.Models;
using Microsoft.Extensions.Logging;

namespace EFlow.Notifications.Application.Email;

public class EmailNotificationService(ILogger<EmailNotificationService> logger) : IEmailNotificationService
{
    public Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = new())
    {
        // TODO
        logger.LogInformation(
            "Email notification to {RecipientEmail}. Subject: {Subject}. Body: {Body}",
            message.RecipientEmail,
            message.Subject,
            message.Body);

        return Task.CompletedTask;
    }
}
