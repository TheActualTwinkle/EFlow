using EFlow.DataImport.Application.Models.Students;

namespace EFlow.DataImport.Application.Services.Students;

public interface IStudentImportWorker
{
    Task<StudentImportProxyResult> ImportStudentsAsync(
        StudentImportProxyRequest request,
        CancellationToken cancellationToken);
}
