using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EC.TaskBoard.Web.Common.Framework.Persistence.Common;

namespace EC.TaskBoard.Web.Common.Framework.Persistence.EFCore;

public sealed class EfCoreSession : ISession
{
    private readonly DbContext _context;

    public EfCoreSession(DbContext context)
    {
        _context = context;
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    public async Task SaveAsync(CancellationToken token)
    {
        await _context.SaveChangesAsync(token);
    }
}
