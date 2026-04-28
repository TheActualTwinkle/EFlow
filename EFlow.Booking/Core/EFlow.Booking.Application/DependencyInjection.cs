using System.Reflection;
using EFlow.Booking.Application.Common.Behaviors;
using EFlow.Booking.Application.Common.Outbox;
using EFlow.Booking.Application.Common.Outbox.Interfaces;
using EFlow.Booking.Application.Services.AdminInitialing;
using EFlow.Booking.Application.Services.AdminInitialing.Interfaces;
using EFlow.Common.Infrastructure;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Booking.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ISystemClock, SystemClock>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(RequestLoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<IOutboxMessageFactory, OutboxMessageFactory>();

        services.Configure<AdminConfiguration>(configuration.GetRequiredSection(AdminConfiguration.SectionName));
        services.AddScoped<IAdminInitializer, AdminInitializer>();

        return services;
    }
}