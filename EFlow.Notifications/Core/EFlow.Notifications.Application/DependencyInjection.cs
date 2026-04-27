using EFlow.Common.Extensions;
using EFlow.Common.IntegrationEvents.Booking.BookingRecords;
using EFlow.Common.IntegrationEvents.Booking.SubmissionSlots;
using EFlow.Common.Infrastructure;
using EFlow.Notifications.Application.BookingReminders;
using EFlow.Notifications.Application.Email;
using EFlow.Notifications.Application.Email.Interfaces;
using EFlow.Notifications.Application.EventHandlers.Processors;
using EFlow.Notifications.Application.EventHandlers.Processors.Interfaces;
using EFlow.Notifications.Messaging.Booking.Settings;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EFlow.Notifications.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ISystemClock, SystemClock>();
        
        // TODO: add settings
        services.AddScoped<IEmailNotificationService, EmailNotificationService>();
        
        services.AddScoped<IIntegrationEventProcessor<SubmissionSlotCreatedIntegrationEvent>, SubmissionSlotCreatedIntegrationEventProcessor>();
        services.AddScoped<IIntegrationEventProcessor<SubmissionSlotUpdatedIntegrationEvent>, SubmissionSlotUpdatedIntegrationEventProcessor>();
        services.AddScoped<IIntegrationEventProcessor<BookingCreatedIntegrationEvent>, BookingCreatedIntegrationEventProcessor>();
        services.AddScoped<IIntegrationEventProcessor<BookingCancelledIntegrationEvent>, BookingCancelledIntegrationEventProcessor>();
        services.AddScoped<BookingReminderJob>();

        return services;
    }

    public static IServiceCollection AddJobScheduler(this IServiceCollection services, IConfiguration configuration)
    {
        var hangfireConnectionString = configuration.GetConnectionString("HangfireDb") ??
                                       throw new InvalidOperationException("Hangfire connection string is not configured.");

        services.AddHangfire(c =>
            c.UseDynamicJobs()
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(o => o.UseNpgsqlConnection(hangfireConnectionString))
                .UseFilter(new AutomaticRetryAttribute { Attempts = 3 }));

        services.AddHangfireServer(o =>
        {
            o.Queues = ["eflow-reminders"];
            o.SchedulePollingInterval = TimeSpan.FromSeconds(10);
        });

        return services;
    }

    public static IApplicationBuilder UseBookingReminders(this WebApplication app)
    {
        var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
        var settings = app.Services.GetRequiredService<IOptions<BookingReminderSettings>>().Value;

        recurringJobManager.AddOrUpdateDynamic<BookingReminderJob>(
            "ProcessBookingReminders",
            job => job.ProcessAsync(new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token),
            settings.PollInterval.ToCronExpression(),
#pragma warning disable CS0618 // Type or member is obsolete
            new DynamicRecurringJobOptions { QueueName = "eflow-reminders" });
#pragma warning restore CS0618 // Type or member is obsolete

        return app;
    }
}
