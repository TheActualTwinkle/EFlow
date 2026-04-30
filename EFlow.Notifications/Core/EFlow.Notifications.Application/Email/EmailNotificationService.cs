using EFlow.Common.Infrastructure;
using EFlow.Notifications.Application.Email.Interfaces;
using EFlow.Notifications.Application.Email.Models;
using EFlow.Notifications.Application.Email.Settings;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Microsoft.Extensions.Logging;

namespace EFlow.Notifications.Application.Email;

public class EmailNotificationService(
    IOptions<SmtpSettings> settingsOptions,
    ISystemClock systemClock,
    ILogger<EmailNotificationService> logger)
    : IEmailNotificationService
{
    public async Task SendAsync(NotificationMessage notificationMessage, CancellationToken cancellationToken = new())
    {
        var smtpSettings = settingsOptions.Value;
        var emailMessage = CreateMimeMessage(notificationMessage, smtpSettings);

        using var client = new SmtpClient();

        await client.ConnectAsync(
            smtpSettings.Host,
            smtpSettings.Port,
            smtpSettings.SecureSocketOptions,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(smtpSettings.Username))
        {
            if (string.IsNullOrWhiteSpace(smtpSettings.Password))
                throw new InvalidOperationException("SMTP password is required when SMTP username is configured.");

            await client.AuthenticateAsync(smtpSettings.Username, smtpSettings.Password, cancellationToken);
        }

        await client.SendAsync(emailMessage, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        logger.LogInformation(
            "Email notification sent to {RecipientEmail}. Subject: {Subject}",
            notificationMessage.RecipientEmail,
            notificationMessage.Subject);
    }

    private MimeMessage CreateMimeMessage(NotificationMessage message, SmtpSettings settings)
    {
        var email = new MimeMessage();
        
        email.From.Add(new MailboxAddress(settings.FromName, settings.FromEmail));
        email.To.Add(MailboxAddress.Parse(message.RecipientEmail));
        email.Subject = message.Subject;
        email.Body = new BodyBuilder { HtmlBody = message.Body }.ToMessageBody();
        email.Date = systemClock.UtcNow;

        return email;
    }
}
