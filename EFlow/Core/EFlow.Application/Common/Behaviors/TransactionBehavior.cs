using System.Data;
using EFlow.Application.Common.Markers;
using EFlow.Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EFlow.Application.Common.Behaviors;

public class TransactionBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork,
    ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ITransactionalRequest
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Beginning transaction for {Request}", typeof(TRequest).Name);

        await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        try
        {
            var response = await next(cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Transaction committed for {Request}", typeof(TRequest).Name);

            return response;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling transaction for {Request}. Performing rollback...", typeof(TRequest).Name);

            try
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
            }
            catch (Exception rollbackException)
            {
                logger.LogError(rollbackException, "Error during transaction rollback for {Request}", typeof(TRequest).Name);

                throw new AggregateException("Error during transaction rollback", e, rollbackException);
            }

            logger.LogInformation("Transaction rolled back for {Request}", typeof(TRequest).Name);

            throw;
        }
    }
}