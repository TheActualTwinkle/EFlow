namespace EFlow.Notifications.Services.NotificationServices.Interfaces;

public interface INotificationService
{
    public Task SendAsync(CancellationToken cancellationToken = new());
}