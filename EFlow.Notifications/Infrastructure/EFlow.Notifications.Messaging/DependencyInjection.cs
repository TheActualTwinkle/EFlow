using Confluent.Kafka;
using EFlow.Common.Messaging.Factories;
using EFlow.Common.Messaging.Serialization;
using EFlow.Common.Messaging.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Notifications.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaSettings>(configuration.GetRequiredSection("KafkaSettings"));

        services.AddScoped<ICommitLogConsumerFactory, CommitLogConsumerFactory>();

        services.AddScoped(typeof(IDeserializer<>), typeof(JsonDeserializer<>));

        return services;
    }
}