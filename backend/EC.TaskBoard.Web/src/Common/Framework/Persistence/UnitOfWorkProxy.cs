using System;
using System.Threading;
using System.Threading.Tasks;
using EC.TaskBoard.Web.Common.Framework.Persistence.Common;

namespace EC.TaskBoard.Web.Common.Framework.Persistence;

internal sealed class UnitOfWorkProxy : IUnitOfWork
{
    private readonly IUnitOfWork _unitOfWork;

    private UnitOfWorkProxy(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task SaveAsync(CancellationToken token)
    {
        await _unitOfWork.SaveAsync(token);
    }

    public async ValueTask DisposeAsync()
    {
        await _unitOfWork.DisposeAsync();
    }

    public async static Task<IUnitOfWork> InstantiateAsync(
        IRepository repository,
        CancellationToken token = default
    )
    {
        if (repository is null)
            throw new ArgumentNullException(nameof(repository));

        return new UnitOfWorkProxy(await repository.InstantiateAsync(token));
    }
}
