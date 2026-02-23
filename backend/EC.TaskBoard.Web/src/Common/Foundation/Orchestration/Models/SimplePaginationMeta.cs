namespace EC.TaskBoard.Web.Common.Foundation.Orchestration.Models;

public sealed record SimplePaginationMeta(
    int Offset,
    int Limit,
    int Total,
    bool HasMore
);
