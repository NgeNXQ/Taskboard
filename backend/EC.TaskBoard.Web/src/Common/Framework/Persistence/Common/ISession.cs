using System;
using System.Threading;
using System.Threading.Tasks;

namespace EC.TaskBoard.Web.Common.Framework.Persistence.Common;

public interface ISession : IAsyncDisposable
{
    Task SaveAsync(CancellationToken token);
}
