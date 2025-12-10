using System.Collections.Concurrent;
using System.Data;
using EFlow.Booking.Domain;
using EFlow.Booking.Domain.Repositories;
using EFlow.Booking.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Booking.Persistence.UnitOfWorkContext;

public sealed class UnitOfWork(
    ApplicationDbContext context,
    IServiceProvider serviceProvider)
    : IUnitOfWork
{
    private readonly ConcurrentDictionary<Type, IRepository> _repositories = new();

    private IDbContextTransaction? _currentTransaction;

    private bool _isTransactionStarted;

    private bool _disposed;

    ~UnitOfWork() =>
        Dispose(false);

    public T GetRepository<T>() where T : IRepository
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(UnitOfWork));

        var type = typeof(T);

        return (T)_repositories.GetOrAdd(type, _ => (T)serviceProvider.GetRequiredService(type));
    }

    public async Task BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(UnitOfWork));

        if (_currentTransaction is not null)
            throw new InvalidOperationException("Transaction already started");

        _currentTransaction = await context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);

        _isTransactionStarted = true;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(UnitOfWork));

        if (_currentTransaction is null)
            throw new InvalidOperationException("No active transaction");

        try
        {
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

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
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

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(UnitOfWork));

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task DisposeTransactionAsync()
    {
        if (_currentTransaction is null)
            return;

        await _currentTransaction.DisposeAsync();

        _currentTransaction = null;
        _isTransactionStarted = false;
    }

    #region Dispose

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncInternal();
        GC.SuppressFinalize(this);
    }

    private async ValueTask DisposeAsyncInternal()
    {
        if (_disposed)
            return;

        if (_isTransactionStarted && _currentTransaction is not null)
            await RollbackTransactionAsync();

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
            if (_isTransactionStarted && _currentTransaction is not null)
                _currentTransaction.Rollback();

            _currentTransaction?.Dispose();

            context.Dispose();

            _repositories.Clear();
        }

        _disposed = true;
    }

    #endregion
}