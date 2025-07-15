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

    private IDbContextTransaction? _currentTransaction;

    ~UnitOfWork() =>
        Dispose(false);

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
        if (_currentTransaction is not null)
            throw new InvalidOperationException("Transaction already started");

        _currentTransaction = await context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = new())
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException("No active transaction");

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = new())
    {
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

    public async Task SaveChangesAsync(CancellationToken cancellationToken = new()) =>
        await context.SaveChangesAsync(cancellationToken);

    private async Task DisposeTransactionAsync()
    {
        if (_currentTransaction is null)
            return;

        await _currentTransaction.DisposeAsync().ConfigureAwait(false);

        _currentTransaction = null;
    }

    #region Dispose

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();

        Dispose(false);
        GC.SuppressFinalize(this);
    }

    private async ValueTask DisposeAsyncCore()
    {
        if (_disposed)
            return;

        await DisposeTransactionAsync();
        await context.DisposeAsync();

        _repositories.Clear();

        _disposed = true;
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _currentTransaction?.Dispose();
            context.Dispose();

            _repositories.Clear();
        }

        _disposed = true;
    }

    #endregion
}