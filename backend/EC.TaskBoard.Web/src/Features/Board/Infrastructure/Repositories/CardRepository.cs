using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using EC.TaskBoard.Web.Common.Framework.Persistence.Common;
using EC.TaskBoard.Web.Common.Framework.Persistence.EFCore;
using EC.TaskBoard.Web.Features.Board.Domain.Entities;
using EC.TaskBoard.Web.Features.Board.Orchestration.Interfaces;
using EC.TaskBoard.Web.Features.Board.Infrastructure.Persistence;

namespace EC.TaskBoard.Web.Features.Board.Infrastructure.Repositories;

internal sealed class CardRepository : EfCoreRepository<Card>, ICardRepository
{
    public CardRepository(
        IDatabaseExceptionTranslator translator,
        BoardDbContext context
    ) : base(translator, IsolationLevel.ReadCommitted, context)
    {
    }

    public void Create(Card entry)
    {
        Dao.Add(entry);
    }

    public void Update(Card entry)
    {
        Dao.Update(entry);
    }

    public void Delete(Card entry)
    {
        Dao.Remove(entry);
    }

    public async Task<Card?> ReadByIdAsync(Guid id, CancellationToken token)
    {
        return await Dao.FindAsync(new[] { id }, token);
    }

    public async Task<IEnumerable<Card>> ReadByListIdAsync(Guid id, CancellationToken token)
    {
        return await Dao
            .Where(entry => entry.ListId == id)
            .OrderBy(entry => entry.DueDate)
            .AsNoTracking()
            .ToListAsync(token);
    }

    public async Task<IEnumerable<Card>> ReadAllAsync(CancellationToken token)
    {
        return await Dao
            .OrderByDescending(entry => entry.DueDate)
            .AsNoTracking()
            .ToListAsync(token);
    }
}
