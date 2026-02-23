using System.Collections.Generic;

namespace EC.TaskBoard.Web.Common.Foundation.Orchestration.Models;

public sealed record SimplePaginatedResult<TData>(
    IEnumerable<TData> Data,
    SimplePaginationMeta Meta
);
