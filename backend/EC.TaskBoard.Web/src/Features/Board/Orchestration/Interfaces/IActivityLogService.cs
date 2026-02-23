using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EC.TaskBoard.Web.Common.Foundation.Orchestration.Models;
using EC.TaskBoard.Web.Features.Board.Orchestration.Models;

namespace EC.TaskBoard.Web.Features.Board.Orchestration.Interfaces;

internal interface IActivityLogService
{
    Task CreateAsync(ActivityLogCreateParams parameters, CancellationToken token);

    Task<SimplePaginatedResult<ActivityLogResult>> ReadAllPaginatedAsync(
        int offset,
        int limit,
        CancellationToken token
    );

    Task<IEnumerable<ActivityLogResult>> ReadAllByCardIdAsync(Guid id, CancellationToken token);
}
