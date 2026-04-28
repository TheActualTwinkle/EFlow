using EFlow.Booking.Domain.Admins;
using EFlow.Common.Infrastructure;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Booking.Application.Admins.Queries;

public class GetAllAdminsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllAdminsQuery, Result<IEnumerable<AdminDto>>>
{
    public async Task<Result<IEnumerable<AdminDto>>> Handle(GetAllAdminsQuery request, CancellationToken cancellationToken)
    {
        var admins = (await unitOfWork
                .GetRepository<IAdminRepository>()
                .GetAllAsync(cancellationToken))
            .Adapt<IEnumerable<AdminDto>>();

        return Result.Ok(admins);
    }
}