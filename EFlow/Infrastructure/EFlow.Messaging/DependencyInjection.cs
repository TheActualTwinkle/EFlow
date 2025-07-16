using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.UsingRabbitMq((_, cfg) =>
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
            });
        });

        return services;
    }
}