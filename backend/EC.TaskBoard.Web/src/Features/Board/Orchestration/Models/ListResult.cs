using System;

namespace EC.TaskBoard.Web.Features.Board.Orchestration.Models;

internal sealed record ListResult(
    Guid Id,
    string Name
);
