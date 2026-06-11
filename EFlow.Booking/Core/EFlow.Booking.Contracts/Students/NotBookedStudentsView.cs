namespace EFlow.Booking.Contracts.Students;

public sealed record NotBookedStudentsView
{
    public required IEnumerable<StudentView> AdmittedStudents { get; init; }
    
    public required IEnumerable<StudentView> NotAdmittedStudents { get; init; }
}