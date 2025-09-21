namespace EFlow.Notifications.Services.NotificationServices.Interfaces;

public interface INotificationService
{
    public Task SendAsync(string message, CancellationToken cancellationToken = new());
}