using EFlow.Services.Background;
using EFlow.Services.Outbox;
using EFlow.Services.Outbox.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddOutbox(this IServiceCollection services, IConfiguration configuration)
    {
        var batchSize = configuration
            .GetRequiredSection("OutboxProcessorSettings")
            .GetValue<int>("BatchSize");

        var processInterval = configuration
            .GetRequiredSection("OutboxProcessorSettings")
            .GetValue<TimeSpan>("ProcessInterval");
        
        var deleteAfter = configuration
            .GetRequiredSection("OutboxProcessorSettings")
            .GetValue<TimeSpan>("DeleteAfter");

        services.AddSingleton<OutboxProcessorSettings>(_ => new OutboxProcessorSettings
        {
            BatchSize = batchSize,
            ProcessInterval = processInterval,
            DeleteAfter = deleteAfter
        });

        services.AddScoped<IOutboxProcessor, OutboxProcessor>();

        services.AddHostedService<OutboxProcessorBackgroundService>();

        return services;
    }
}