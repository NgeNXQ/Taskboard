using System;

namespace EC.TaskBoard.Web.Features.Board.Orchestration.Models;

internal sealed record ListUpdateParams(
    Guid Id,
    string Name
);
