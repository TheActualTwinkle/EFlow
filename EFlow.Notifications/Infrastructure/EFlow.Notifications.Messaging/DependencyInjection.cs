using EFlow.Notifications.Messaging.Consumers;
using EFlow.Notifications.Messaging.Consumers.Settings;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            
            x.AddConsumer<SubmissionSlotCreatedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var configurationSection = configuration.GetRequiredSection("RabbitMqSettings");
                var host = configurationSection["Host"]!;
                var username = configurationSection["Username"]!;
                var password = configurationSection["Password"]!;

                cfg.Host(host, h =>
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