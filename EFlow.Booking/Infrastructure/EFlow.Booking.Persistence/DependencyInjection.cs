using EFlow.Booking.Contracts.Admins;
using EFlow.Booking.Contracts.BookingRecords;
using EFlow.Booking.Contracts.Groups;
using EFlow.Booking.Contracts.Students;
using EFlow.Booking.Contracts.Subjects;
using EFlow.Booking.Contracts.SubmissionSlots;
using EFlow.Booking.Contracts.Teachers;
using EFlow.Booking.Domain.Admins;
using EFlow.Booking.Domain.BookingRecords;
using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.Domain.Teachers;
using EFlow.Booking.Domain.Subjects;
using EFlow.Booking.Persistence.DatabaseContext;
using EFlow.Booking.Persistence.QueryServices;
using EFlow.Booking.Persistence.Repositories;
using EFlow.Common.Domain.Repositories;
using EFlow.Common.Infrastructure;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Booking.Persistence;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddPersistence(IConfiguration configuration)
        {
            services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            var connectionString = configuration.GetConnectionString("Database");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    connectionString,
                    builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        
            services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

            services.AddRepositories();
            services.AddQueryServices();

            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            return services;
        }

        public IServiceCollection AddJobScheduler(IConfiguration configuration)
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

        private void AddRepositories()
        {
            services.AddScoped<IAdminRepository, AdminRepository>();
            services.AddScoped<ITeacherRepository, TeacherRepository>();
            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<ISubjectRepository, SubjectRepository>();
            services.AddScoped<ISubmissionSlotRepository, SubmissionSlotRepository>();
            services.AddScoped<IBookingRecordRepository, BookingRecordRepository>();
            services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        }

        private void AddQueryServices()
        {
            services.AddScoped<IAdminQueryService, AdminQueryService>();
            services.AddScoped<ITeacherQueryService, TeacherQueryService>();
            services.AddScoped<IStudentQueryService, StudentQueryService>();
            services.AddScoped<IGroupQueryService, GroupQueryService>();
            services.AddScoped<ISubjectQueryService, SubjectQueryService>();
            services.AddScoped<ISubmissionSlotQueryService, SubmissionSlotQueryService>();
            services.AddScoped<IBookingRecordQueryService, BookingRecordQueryService>();
        }
    }
}
