using System.Data;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Common.Infrastructure;

public class UnitOfWorkFactory(IServiceProvider serviceProvider) : IUnitOfWorkFactory
{
    public async Task<IUnitOfWork> CreateTransactionalAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = new())
    {
        var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();

        await unitOfWork.BeginTransactionAsync(isolationLevel, cancellationToken);

        return unitOfWork;
    }

    public Task<IUnitOfWork> CreateNonTransactionalAsync(CancellationToken cancellationToken = new())
    {
        var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();

        return Task.FromResult(unitOfWork);
    }
}