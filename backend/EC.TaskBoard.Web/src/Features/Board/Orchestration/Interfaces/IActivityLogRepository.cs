using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EC.TaskBoard.Web.Common.Framework.Persistence.Common;
using EC.TaskBoard.Web.Features.Board.Domain.Entities;

namespace EC.TaskBoard.Web.Features.Board.Orchestration.Interfaces;

internal interface IActivityLogRepository : IRepository
{
    void Create(ActivityLog entry);

    Task<IEnumerable<ActivityLog>> ReadAllByCardIdAsync(Guid id, CancellationToken token);

    Task<(IEnumerable<ActivityLog> Data, int Total)> ReadAllPaginatedAsync(
        int offset,
        int limit,
        CancellationToken token
    );
}
