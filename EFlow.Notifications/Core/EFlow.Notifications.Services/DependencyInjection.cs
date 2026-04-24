using EFlow.Notifications.Services.NotificationServices;
using EFlow.Notifications.Services.NotificationServices.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EFlow.Notifications.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IEmailNotificationService, EmailNotificationService>();

        services.Configure<BookingReminderOptions>(configuration.GetSection(BookingReminderOptions.SectionName));

        services.AddHttpClient<BookingReminderSnapshotClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<BookingReminderOptions>>().Value;
            client.BaseAddress = new Uri(options.BookingApiBaseUrl);
        });

        services.AddHostedService<BookingNotificationWorkflowHostedService>();
        services.AddHostedService<BookingReminderBackgroundService>();

        return services;
    }
}
