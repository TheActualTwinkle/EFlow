using FluentResults;
using MediatR;

namespace EFlow.Application.Admins.Queries;

public record GetAllAdminsQuery : IRequest<Result<IEnumerable<AdminDto>>>;