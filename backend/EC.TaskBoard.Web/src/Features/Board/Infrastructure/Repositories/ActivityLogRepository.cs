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

internal sealed class ActivityLogRepository : EfCoreRepository<ActivityLog>, IActivityLogRepository
{
    public ActivityLogRepository(
        IDatabaseExceptionTranslator translator,
        BoardDbContext context
    ) : base(translator, IsolationLevel.ReadCommitted, context)
    {
    }

    public void Create(ActivityLog entry)
    {
        Dao.Add(entry);
    }

    public async Task<IEnumerable<ActivityLog>> ReadAllByCardIdAsync(
        Guid id,
        CancellationToken token)
    {
        return await Dao
            .Where(entry => entry.CardId == id)
            .OrderByDescending(entry => entry.CreatedAt)
            .AsNoTracking()
            .ToListAsync(token);
    }

    public async Task<(IEnumerable<ActivityLog> Data, int Total)> ReadAllPaginatedAsync(
        int offset,
        int limit,
        CancellationToken token)
    {
        var query = Dao.OrderBy(entry => entry.CreatedAt).AsNoTracking();

        var total = await query.CountAsync(token);

        var data = await query
            .Skip(offset)
            .Take(limit)
            .ToListAsync(token);

        return (data, total);
    }
}
