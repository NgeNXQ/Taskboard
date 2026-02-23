using System.Threading;
using System.Threading.Tasks;

namespace EC.TaskBoard.Web.Common.Framework.Persistence.Common;

public interface IRepository
{
    Task<IUnitOfWork> InstantiateAsync(CancellationToken token = default);
}
