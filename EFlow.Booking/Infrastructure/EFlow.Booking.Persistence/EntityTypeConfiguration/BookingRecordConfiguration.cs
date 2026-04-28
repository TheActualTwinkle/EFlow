using EFlow.Booking.Domain.BookingRecords;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.SubmissionSlots;
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
            .HasConversion(
                id => id.Value, 
                value => new BookingRecordId(value))
            .HasColumnName("id");

        builder.Property(b => b.StudentId)
            .HasConversion(
                id => id.Value,
                value => new StudentId(value))
            .HasColumnName("student_id")
            .IsRequired();

        builder.Property(b => b.SlotId)
            .HasConversion(
                id => id.Value,
                value => new SubmissionSlotId(value))
            .HasColumnName("slot_id")
            .IsRequired();

        builder.Property(b => b.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(b => new { b.StudentId, b.SlotId })
            .IsUnique()
            .HasDatabaseName("ix_booking_records_student_id_slot_id");

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(bookingRecord => bookingRecord.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<SubmissionSlot>()
            .WithMany()
            .HasForeignKey(bookingRecord => bookingRecord.SlotId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
