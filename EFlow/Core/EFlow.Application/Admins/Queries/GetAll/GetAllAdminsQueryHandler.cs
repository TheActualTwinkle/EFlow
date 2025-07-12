using EFlow.Domain;
using EFlow.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Application.Admins.Queries;

public class GetAllAdminsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllAdminsQuery, Result<IEnumerable<AdminDto>>>
{
    public Task<Result<IEnumerable<AdminDto>>> Handle(GetAllAdminsQuery request, CancellationToken cancellationToken)
    {
        var admins = unitOfWork
            .GetRepository<IAdminRepository>()
            .GetAllAsync(cancellationToken)
            .Adapt<IEnumerable<AdminDto>>();

        return Task.FromResult(Result.Ok(admins));
    }
}