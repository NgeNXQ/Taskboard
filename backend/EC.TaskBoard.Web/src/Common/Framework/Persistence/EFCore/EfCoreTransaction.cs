using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using EC.TaskBoard.Web.Common.Framework.Persistence.Common;

namespace EC.TaskBoard.Web.Common.Framework.Persistence.EFCore;

internal sealed class EfCoreTransaction : ITransaction
{
    private readonly IDbContextTransaction _transaction;

    public EfCoreTransaction(IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    public async ValueTask DisposeAsync()
    {
        await _transaction.DisposeAsync();
    }

    public async Task CommitAsync(CancellationToken token)
    {
        await _transaction.CommitAsync(token);
    }

    public async Task RollbackAsync(CancellationToken token)
    {
        await _transaction.RollbackAsync(token);
    }
}
