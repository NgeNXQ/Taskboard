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

internal sealed class ListRepository : EfCoreRepository<List>, IListRepository
{
    public ListRepository(
        IDatabaseExceptionTranslator translator,
        BoardDbContext context
    ) : base(translator, IsolationLevel.ReadCommitted, context)
    {
    }

    public void Create(List entry)
    {
        Dao.Add(entry);
    }

    public void Update(List entry)
    {
        Dao.Update(entry);
    }

    public void Delete(List entry)
    {
        Dao.Remove(entry);
    }

    public async Task<List?> ReadByIdAsync(Guid id, CancellationToken token)
    {
        return await Dao.FindAsync(new[] { id }, token);
    }

    public async Task<IEnumerable<List>> ReadAllAsync(CancellationToken token)
    {
        return await Dao.OrderBy(entry => entry.CreatedAt).AsNoTracking().ToListAsync();
    }

    public async Task<bool> DoesExistWithIdAsync(Guid id, CancellationToken token)
    {
        return await Dao.AnyAsync(entry => entry.Id == id, token);
    }
}
