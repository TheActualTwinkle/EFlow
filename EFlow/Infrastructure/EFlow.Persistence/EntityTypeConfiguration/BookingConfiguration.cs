using EFlow.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFlow.Persistence.EntityTypeConfiguration;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");

        builder.HasKey(b => b.Id)
            .HasName("pk_bookings");

        builder.Property(b => b.Id)
            .HasColumnName("id");

        builder.Property(b => b.StudentId)
            .HasColumnName("student_id")
            .IsRequired();

        builder.Property(b => b.SlotId)
            .HasColumnName("slot_id")
            .IsRequired();

        builder.Property(b => b.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(b => new { b.StudentId, b.SlotId })
            .IsUnique()
            .HasDatabaseName("ix_bookings_student_id_slot_id");

        builder.HasOne(b => b.Student)
            .WithMany()
            .HasForeignKey(b => b.StudentId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_bookings_students");

        builder.HasOne(b => b.SubmissionSlot)
            .WithMany()
            .HasForeignKey(b => b.SlotId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_bookings_submission_slots");
    }
}