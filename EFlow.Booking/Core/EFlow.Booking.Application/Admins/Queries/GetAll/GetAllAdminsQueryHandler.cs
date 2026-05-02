using EFlow.Booking.Contracts.Admins;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Admins.Queries;

public class GetAllAdminsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllAdminsQuery, Result<IEnumerable<AdminView>>>
{
    public async Task<Result<IEnumerable<AdminView>>> Handle(GetAllAdminsQuery request, CancellationToken cancellationToken)
    {
        var admins = await unitOfWork
            .GetQueryService<IAdminQueryService>()
            .GetAllAsync(cancellationToken);

        return Result.Ok(admins);
    }
}