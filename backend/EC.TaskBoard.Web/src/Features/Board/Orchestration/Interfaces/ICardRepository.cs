using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EC.TaskBoard.Web.Common.Framework.Persistence.Common;
using EC.TaskBoard.Web.Features.Board.Domain.Entities;

namespace EC.TaskBoard.Web.Features.Board.Orchestration.Interfaces;

internal interface ICardRepository : IRepository
{
    public void Create(Card entry);
    public void Update(Card entry);
    public void Delete(Card entry);

    Task<Card?> ReadByIdAsync(Guid id, CancellationToken token);
    Task<IEnumerable<Card>> ReadByListIdAsync(Guid id, CancellationToken token);
    Task<IEnumerable<Card>> ReadAllAsync(CancellationToken token);
}
