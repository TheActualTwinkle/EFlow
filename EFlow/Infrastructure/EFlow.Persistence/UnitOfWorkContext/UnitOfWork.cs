using System.Collections.Concurrent;
using System.Data;
using EFlow.Domain;
using EFlow.Domain.Repositories;
using EFlow.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Persistence.UnitOfWorkContext;

public sealed class UnitOfWork(
    ApplicationDbContext context,
    IServiceProvider serviceProvider)
    : IUnitOfWork
{
    private readonly ConcurrentDictionary<Type, IRepository> _repositories = new();

    private bool _disposed;

    private IDbContextTransaction? CurrentTransaction =>
        context.Database.CurrentTransaction;

    public T GetRepository<T>() where T : IRepository
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(UnitOfWork));

        var type = typeof(T);

        return (T)_repositories.GetOrAdd(
            type, _ =>
            {
                var repo = (T)serviceProvider.GetRequiredService(type);

                return repo;
            });
    }

    public async Task BeginAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = new())
    {
        if (CurrentTransaction is not null)
            throw new InvalidOperationException("Transaction already started");

        await context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = new())
    {
        if (CurrentTransaction is null)
            throw new InvalidOperationException("No active transaction");

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            await CurrentTransaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = new())
    {
        if (CurrentTransaction is null)
            return;

        try
        {
            await CurrentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = new()) =>
        await context.SaveChangesAsync(cancellationToken);

    ~UnitOfWork() =>
        Dispose(false);

    #region Dispose

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);

        Dispose(false);
        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private async Task DisposeTransactionAsync()
    {
        if (CurrentTransaction is not null)
            await CurrentTransaction.DisposeAsync().ConfigureAwait(false);
    }

    private async ValueTask DisposeAsyncCore()
    {
        if (CurrentTransaction is not null)
            await CurrentTransaction.DisposeAsync().ConfigureAwait(false);

        await context.DisposeAsync();
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _repositories.Clear();

            CurrentTransaction?.Dispose();
            context.Dispose();
        }

        _disposed = true;
    }

    #endregion
}