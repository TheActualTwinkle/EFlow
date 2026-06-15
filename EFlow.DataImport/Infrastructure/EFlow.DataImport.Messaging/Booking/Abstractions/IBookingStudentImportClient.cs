using EFlow.DataImport.Messaging.Booking.Models;

namespace EFlow.DataImport.Messaging.Booking.Abstractions;

public interface IBookingStudentImportClient
{
    Task<BookingStudentImportResult> ImportStudentsAsync(
        Guid groupId,
        IReadOnlyList<BookingImportedStudent> students,
        CancellationToken cancellationToken);
}
