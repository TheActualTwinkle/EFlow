using EFlow.Notifications.Services.NotificationServices.Interfaces;

namespace EFlow.Notifications.Services.NotificationServices;

public class EmailNotificationService : INotificationService
{
    public Task SendAsync(CancellationToken cancellationToken = new())
    {
        Console.WriteLine("Testing Email Notification Service");
        
        return Task.CompletedTask;
    }
}