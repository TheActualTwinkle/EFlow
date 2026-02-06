using EFlow.Booking.Domain;
using EFlow.Booking.Persistence.DatabaseContext;
using EFlow.Booking.Persistence.Repositories;
using EFlow.Booking.Domain.Repositories;
using EFlow.Common.Infrastructure;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Booking.Persistence;

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
        
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<ITeacherRepository, TeacherRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<ISubmissionSlotRepository, SubmissionSlotRepository>();
        services.AddScoped<IBookingRecordRepository, BookingRecordRepository>();
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