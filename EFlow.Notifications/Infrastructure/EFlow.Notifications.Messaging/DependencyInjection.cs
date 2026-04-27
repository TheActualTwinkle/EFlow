using Confluent.Kafka;
using EFlow.Common.Messaging.Factories;
using EFlow.Common.Messaging.Init;
using EFlow.Common.Messaging.Serialization;
using EFlow.Common.Messaging.Settings;
using EFlow.Notifications.Messaging.Booking;
using EFlow.Notifications.Messaging.Booking.Interfaces;
using EFlow.Notifications.Messaging.Booking.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EFlow.Notifications.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<TopicInitializer>();

        services.Configure<KafkaSettings>(configuration.GetRequiredSection("KafkaSettings"));
        services.Configure<KafkaTopicsSettings>(configuration.GetRequiredSection("KafkaSettings"));

        services.AddScoped<ICommitLogConsumerFactory, CommitLogConsumerFactory>();

        services.AddSingleton<IAdminClient>(serviceProvider =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<KafkaSettings>>().Value;

            return new AdminClientBuilder(new AdminClientConfig { BootstrapServers = settings.BootstrapServers }).Build();
        });

        services.AddScoped(typeof(IDeserializer<>), typeof(DefaultSerializer<>));

        return services;
    }

    public static IServiceCollection AddBookingClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BookingReminderSettings>(configuration.GetSection(BookingReminderSettings.SectionName));

        services.AddHttpClient<IBookingClient, BookingClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<BookingReminderSettings>>().Value;
            
            client.BaseAddress = new Uri(options.BookingApiBaseUrl);
        });
        
        return services;
    }
}
