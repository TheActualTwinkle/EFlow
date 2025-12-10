using EFlow.Booking.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFlow.Booking.Persistence.EntityTypeConfiguration;

public class BookingRecordConfiguration : IEntityTypeConfiguration<BookingRecord>
{
    public void Configure(EntityTypeBuilder<BookingRecord> builder)
    {
        builder.ToTable("booking_records");

        builder.HasKey(b => b.Id)
            .HasName("pk_booking_records");

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
            .HasDatabaseName("ix_booking_records_student_id_slot_id");

        builder.HasOne(b => b.Student)
            .WithMany()
            .HasForeignKey(b => b.StudentId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_booking_records_students");

        builder.HasOne(b => b.SubmissionSlot)
            .WithMany()
            .HasForeignKey(b => b.SlotId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_booking_records_submission_slots");
    }
}