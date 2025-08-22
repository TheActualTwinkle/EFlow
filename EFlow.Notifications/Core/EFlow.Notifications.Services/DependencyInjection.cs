using EFlow.Notifications.Services.NotificationServices;
using EFlow.Notifications.Services.NotificationServices.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Notifications.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationServices(this IServiceCollection services)
    {
        services.AddHostedService<EmailNotificationService>();

        return services;
    }
}