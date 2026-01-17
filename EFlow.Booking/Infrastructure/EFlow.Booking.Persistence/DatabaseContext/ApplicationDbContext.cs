using EFlow.Common.Domain.Entities;
using EFlow.Common.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.DatabaseContext;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<Identity, IdentityRole<Guid>, Guid>(options)
{
    public required DbSet<Admin> Admins { get; init; }

    public required DbSet<Teacher> Teachers { get; init; }

    public required DbSet<Student> Students { get; init; }

    public required DbSet<Group> Groups { get; init; }

    public required DbSet<Subject> Subjects { get; init; }

    public required DbSet<SubmissionSlot> SubmissionSlots { get; init; }

    public required DbSet<BookingRecord> BookingRecords { get; init; }

    public required DbSet<OutboxMessage> OutboxMessages { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureIdentityModels(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    private static void ConfigureIdentityModels(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            if (entityType.ClrType.Namespace?.Contains("Microsoft.AspNetCore.Identity") == true)
                entityType.SetSchema("identity");

        modelBuilder.Entity<Identity>().ToTable("AspNetUsers", "identity");
    }
}