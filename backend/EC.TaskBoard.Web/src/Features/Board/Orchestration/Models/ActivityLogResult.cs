using EC.TaskBoard.Web.Features.Board.Domain.Enums;

namespace EC.TaskBoard.Web.Features.Board.Orchestration.Models;

internal sealed record ActivityLogResult(
    ActivityLogKind Kind,
    string Current,
    string Previous
);
