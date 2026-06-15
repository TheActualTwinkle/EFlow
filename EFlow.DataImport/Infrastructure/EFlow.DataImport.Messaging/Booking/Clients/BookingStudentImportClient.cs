using System.Net.Http.Json;
using EFlow.DataImport.Messaging.Booking.Abstractions;
using EFlow.DataImport.Messaging.Booking.Contracts;
using EFlow.DataImport.Messaging.Booking.Models;

namespace EFlow.DataImport.Messaging.Booking.Clients;

public sealed class BookingStudentImportClient(HttpClient httpClient)
    : IBookingStudentImportClient
{
    public async Task<BookingStudentImportResult> ImportStudentsAsync(
        Guid groupId,
        IReadOnlyList<BookingImportedStudent> students,
        CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"api/internal/import/students?groupId={groupId}");

        httpRequest.Content = JsonContent.Create(new ImportStudentsRequest { Students = students });

        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.ToString();

        return new BookingStudentImportResult
        {
            StatusCode = response.StatusCode,
            Body = body,
            ContentType = contentType
        };
    }
}
