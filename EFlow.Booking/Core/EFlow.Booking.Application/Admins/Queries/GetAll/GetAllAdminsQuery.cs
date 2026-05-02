using EFlow.Booking.Contracts.Admins;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Admins.Queries;

public record GetAllAdminsQuery : IRequest<Result<IEnumerable<AdminView>>>;