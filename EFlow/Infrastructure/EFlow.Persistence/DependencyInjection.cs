using EFlow.Domain;
using EFlow.Domain.Repositories;
using EFlow.Persistence.DatabaseContext;
using EFlow.Persistence.Repositories;
using EFlow.Persistence.UnitOfWorkContext;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        var connectionString = configuration.GetConnectionString("Database");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<ITeacherRepository, TeacherRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<ISubmissionSlotRepository, SubmissionSlotRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();

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
            o.Queues = ["eflow-outbox"];
            o.SchedulePollingInterval = TimeSpan.FromSeconds(10);
        });

        return services;
    }
}