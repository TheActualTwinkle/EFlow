namespace EFlow.Notifications.Services.NotificationServices.Interfaces;

public interface IEmailNotificationService
{
    public Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = new());
}
