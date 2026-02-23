using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EC.TaskBoard.Web.Common.Framework.Persistence.Common;

namespace EC.TaskBoard.Web.Common.Framework.Persistence.EFCore;

internal sealed class EfCoreUnitOfWork : IUnitOfWork
{
    private readonly ISession _session;
    private readonly ITransaction _transaction;
    private readonly IDatabaseExceptionTranslator _translator;
    private readonly CancellationToken _token;

    internal EfCoreUnitOfWork(
        ISession session,
        ITransaction transaction,
        IDatabaseExceptionTranslator translator,
        CancellationToken token
    )
    {
        _token = token;
        _session = session;
        _translator = translator;
        _transaction = transaction;
    }

    public async Task SaveAsync(CancellationToken token)
    {
        try
        {
            await _session.SaveAsync(token);
        }
        catch (DbUpdateException ex)
        {
            _translator.ThrowInnerException(ex.InnerException);
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _transaction.CommitAsync(_token);
        }
        catch
        {
            await _transaction.RollbackAsync(_token);
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
        }
    }
}
