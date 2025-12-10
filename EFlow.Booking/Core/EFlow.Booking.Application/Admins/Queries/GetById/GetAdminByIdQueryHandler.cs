using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Domain;
using EFlow.Booking.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Booking.Application.Admins.Queries;

public class GetAdminByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAdminByIdQuery, Result<AdminDto>>
{
    public async Task<Result<AdminDto>> Handle(GetAdminByIdQuery request, CancellationToken cancellationToken)
    {
        var admin = await unitOfWork
            .GetRepository<IAdminRepository>()
            .GetByIdAsync(request.Id, cancellationToken);

        if (admin is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Admin not found")
                    .WithId(request.Id));

        return admin.Adapt<AdminDto>();
    }
}