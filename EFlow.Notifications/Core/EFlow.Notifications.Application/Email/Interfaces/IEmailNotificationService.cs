using EFlow.Notifications.Application.Email.Models;

namespace EFlow.Notifications.Application.Email.Interfaces;

public interface IEmailNotificationService
{
    public Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = new());
}
