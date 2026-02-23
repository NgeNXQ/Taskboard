using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EC.TaskBoard.Web.Common.Framework.Persistence.Common;
using EC.TaskBoard.Web.Common.Foundation.Domain;

namespace EC.TaskBoard.Web.Common.Framework.Persistence.EFCore;

public abstract class EfCoreRepository<TEntity> : IRepository
    where TEntity : class, IEntity
{
    private readonly DbContext _context;
    private readonly IsolationLevel _level;
    private readonly IDatabaseExceptionTranslator _translator;

    public EfCoreRepository(
        IDatabaseExceptionTranslator translator,
        IsolationLevel level,
        DbContext context
    )
    {
        _level = level;
        _context = context;
        _translator = translator;
        Dao = context.Set<TEntity>();
    }

    protected DbSet<TEntity> Dao { get; }

    public async Task<IUnitOfWork> InstantiateAsync(CancellationToken token = default)
    {
        return new EfCoreUnitOfWork(
            new EfCoreSession(_context),
            new EfCoreTransaction(await _context.Database.BeginTransactionAsync(_level, token)),
            _translator,
            token
        );
    }
}
