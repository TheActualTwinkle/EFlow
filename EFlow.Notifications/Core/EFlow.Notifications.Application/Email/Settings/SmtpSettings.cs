using MailKit.Security;

namespace EFlow.Notifications.Application.Email.Settings;

public sealed record SmtpSettings
{
    public const string SectionName = "Smtp";

    public required string Host { get; init; }

    public required int Port { get; init; }

    public string? Username { get; init; }

    public string? Password { get; init; }

    public required string FromEmail { get; init; }

    public required string FromName { get; init; }

    public required SecureSocketOptions SecureSocketOptions { get; init; }
}
