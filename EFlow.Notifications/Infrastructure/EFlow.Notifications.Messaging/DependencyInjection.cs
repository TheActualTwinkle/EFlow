using EFlow.Notifications.Messaging.Consumers;
using EFlow.Notifications.Messaging.Consumers.Settings;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Notifications.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.AddConsumer<SubmissionSlotCreatedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var configurationSection = configuration.GetRequiredSection("RabbitMqSettings");

                var host = configurationSection["Host"] ??
                           throw new InvalidOperationException("RabbitMqSettings:Host is not configured.");

                var port = ushort.Parse(
                    configurationSection["Port"] ??
                    throw new InvalidOperationException("RabbitMqSettings:Port is not configured."));

                var username = configurationSection["Username"] ??
                               throw new InvalidOperationException("RabbitMqSettings:Username is not configured.");

                var password = configurationSection["Password"] ??
                               throw new InvalidOperationException("RabbitMqSettings:Password is not configured.");

                cfg.Host(
                    host, port, "/", h =>
                    {
                        h.Username(username);
                        h.Password(password);
                    });

                cfg.ReceiveEndpoint(
                    "eflow-submission-slot-created",
                    e => e.Consumer<SubmissionSlotCreatedConsumer>(context));
            });
        });

        AddConsumerSettings(services, configuration);

        return services;
    }

    private static void AddConsumerSettings(IServiceCollection services, IConfiguration configuration)
    {
        var defaultCancellationTimeoutString = configuration
            .GetRequiredSection("ConsumersSettings")["DefaultCancellationTimeout"];

        if (string.IsNullOrEmpty(defaultCancellationTimeoutString))
            throw new InvalidOperationException("DefaultCancellationTimeout is not configured in ConsumersSettings.");

        var defaultCancellationTimeout = TimeSpan.Parse(defaultCancellationTimeoutString);

        services.AddSingleton<ConsumerSettings>(_ => new ConsumerSettings
        {
            DefaultCancellationTimeout = defaultCancellationTimeout
        });
    }
}