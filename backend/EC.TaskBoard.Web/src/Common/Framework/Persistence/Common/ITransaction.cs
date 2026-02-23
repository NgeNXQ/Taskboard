using System;
using System.Threading;
using System.Threading.Tasks;

namespace EC.TaskBoard.Web.Common.Framework.Persistence.Common;

public interface ITransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken token);
    Task RollbackAsync(CancellationToken token);
}
