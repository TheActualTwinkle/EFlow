using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Teachers;
using EFlow.Booking.Domain.Subjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFlow.Booking.Persistence.EntityTypeConfiguration;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("subjects");

        builder.HasKey(s => s.Id)
            .HasName("pk_subjects");

        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, value => new SubjectId(value))
            .HasColumnName("id");

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(127)
            .IsRequired();

        builder.Property(s => s.TeacherId)
            .HasConversion(
                id => id.Value,
                value => new TeacherId(value))
            .HasColumnName("teacher_id")
            .IsRequired();

        builder.Property(s => s.GroupIds)
            .HasConversion(
                ids => ids.Select(x => x.Value).ToArray(),
                values => values
                    .Select(value => new GroupId(value))
                    .ToArray())
            .Metadata.SetValueComparer(
                new ValueComparer<ICollection<GroupId>>(
                    (left, right) => left!.SequenceEqual(right!),
                    ids => ids.Aggregate(0, (hash, id) => HashCode.Combine(hash, id.Value.GetHashCode())),
                    ids => ids.ToArray()));
        
        builder.Property(s => s.GroupIds)
            .HasColumnName("group_ids")
            .HasColumnType("uuid[]")
            .IsRequired();
    }
}