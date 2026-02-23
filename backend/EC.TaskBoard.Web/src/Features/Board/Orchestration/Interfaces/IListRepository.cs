using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EC.TaskBoard.Web.Common.Framework.Persistence.Common;
using EC.TaskBoard.Web.Features.Board.Domain.Entities;

namespace EC.TaskBoard.Web.Features.Board.Orchestration.Interfaces;

internal interface IListRepository : IRepository
{
    public void Create(List entry);
    public void Update(List entry);
    public void Delete(List entry);

    Task<List?> ReadByIdAsync(Guid id, CancellationToken token);
    Task<IEnumerable<List>> ReadAllAsync(CancellationToken token);

    Task<bool> DoesExistWithIdAsync(Guid id, CancellationToken token);
}
