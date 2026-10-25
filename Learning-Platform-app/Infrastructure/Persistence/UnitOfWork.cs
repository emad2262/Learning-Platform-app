using Learning_Platform_app.Common.Abstractions;
using Learning_Platform_app.Common.Result_pattern;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;

namespace Learning_Platform_app.Infrastructure.Persistence;

public class UnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction _transaction;
    private int _depth ;
    private bool _rollbackOnly;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;

    }

    public async Task ExcuteAsync(Func<Task> action)
    {
        bool isOuter = _depth == 0;
        // if i'm in the outer begin transaction
        if (isOuter)
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }
        _depth++;
        //excute coding and save 
        try
        {
            await action();

            // Inner execution doesn't own the commit
            if (!isOuter)
                return;

            // An inner operation failed
            if (_rollbackOnly)
            {
                throw new InvalidOperationException("Transaction marked for rollback.");
            }

            await _context.SaveChangesAsync();

            await _transaction!.CommitAsync();
        }
        catch
        {
            _rollbackOnly = true;

            if (isOuter && _transaction != null)
            {
                await _transaction.RollbackAsync(
                    CancellationToken.None);
            }

            throw;
        }
        finally
        {
            _depth--;

            if (isOuter)
            {
                await _transaction.DisposeAsync();

                _transaction = null;
                _rollbackOnly = false;

                
            }
        }
    }
    public async Task<int> SaveChangeAsync()
    {
        return await _context.SaveChangesAsync();
    }
}

