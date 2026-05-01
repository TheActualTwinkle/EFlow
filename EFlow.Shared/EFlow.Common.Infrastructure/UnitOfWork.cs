using System.Data;
using EFlow.Common.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Common.Infrastructure;

public sealed class UnitOfWork(
    DbContext context,
    IServiceProvider serviceProvider,
    IDomainEventDispatcher domainEventsDispatcher)
    : IUnitOfWork
{
    private IDbContextTransaction? _currentTransaction;

    private bool _isTransactionStarted;

    private bool _disposed;

    public T GetRepository<T>() where T : IRepository
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(UnitOfWork));

        var type = typeof(T);

        return (T)serviceProvider.GetRequiredService(type);
    }

    public T GetQueryService<T>() where T : IQueryService
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(UnitOfWork));

        var type = typeof(T);

        return (T)serviceProvider.GetRequiredService(type);
    }

    public async Task BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = new())
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(UnitOfWork));

        if (_currentTransaction is not null)
            throw new InvalidOperationException("Transaction already started");

        _currentTransaction = await context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);

        _isTransactionStarted = true;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = new())
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(UnitOfWork));

        if (_currentTransaction is null)
            throw new InvalidOperationException("No active transaction");

        try
        {
            await domainEventsDispatcher.DispatchEventsAsync(cancellationToken);
            
            await context.SaveChangesAsync(cancellationToken);

            await _currentTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);

            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = new())
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(UnitOfWork));

        if (_currentTransaction is null)
            return;

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        if (_isTransactionStarted && _currentTransaction is not null)
            await RollbackTransactionAsync();

        await context.DisposeAsync();

        _disposed = true;
    }

    private async Task DisposeTransactionAsync()
    {
        if (_currentTransaction is null)
            return;

        await _currentTransaction.DisposeAsync();

        _currentTransaction = null;
        _isTransactionStarted = false;
    }
}