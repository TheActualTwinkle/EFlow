using EFlow.Common.Domain;

namespace EFlow.Booking.Subjects;

public sealed record SubjectId(Guid Value) : TypedIdValueBase(Value);