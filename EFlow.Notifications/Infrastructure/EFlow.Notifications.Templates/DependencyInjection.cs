using EFlow.Notifications.Templates.Notifications;
using EFlow.Notifications.Templates.Notifications.Interfaces;
using EFlow.Notifications.Templates.Rendering;
using EFlow.Notifications.Templates.Rendering.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Notifications.Templates;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationsTemplates(this IServiceCollection services)
    {
        services.AddRazorPages();
        
        services.AddScoped<ITemplateRenderer, RazorTemplateRenderer>();
        services.AddScoped<IBookingNotificationTemplateService, BookingNotificationTemplateService>();

        return services;
    }
}