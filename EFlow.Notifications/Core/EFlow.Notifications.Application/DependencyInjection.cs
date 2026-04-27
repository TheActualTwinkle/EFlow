using EFlow.Common.IntegrationEvents.Booking.BookingRecords;
using EFlow.Common.IntegrationEvents.Booking.SubmissionSlots;
using EFlow.Notifications.Application.Email;
using EFlow.Notifications.Application.Email.Interfaces;
using EFlow.Notifications.Application.EventHandlers.Processors;
using EFlow.Notifications.Application.EventHandlers.Processors.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Notifications.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: add settings
        services.AddScoped<IEmailNotificationService, EmailNotificationService>();
        
        services.AddScoped<IIntegrationEventProcessor<SubmissionSlotCreatedIntegrationEvent>, SubmissionSlotCreatedIntegrationEventProcessor>();
        services.AddScoped<IIntegrationEventProcessor<SubmissionSlotUpdatedIntegrationEvent>, SubmissionSlotUpdatedIntegrationEventProcessor>();
        services.AddScoped<IIntegrationEventProcessor<BookingCreatedIntegrationEvent>, BookingCreatedIntegrationEventProcessor>();
        services.AddScoped<IIntegrationEventProcessor<BookingCancelledIntegrationEvent>, BookingCancelledIntegrationEventProcessor>();

        return services;
    }
}
