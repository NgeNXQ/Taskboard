using System;
using EC.TaskBoard.Web.Features.Board.Domain.Enums;

namespace EC.TaskBoard.Web.Features.Board.Orchestration.Models;

internal sealed record CardResult(
    Guid Id,
    Guid ListId,
    string Name,
    string Description,
    CardPriority Priority,
    DateTimeOffset DueDate
);
