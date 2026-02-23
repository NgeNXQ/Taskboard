using System;
using EC.TaskBoard.Web.Features.Board.Domain.Enums;

namespace EC.TaskBoard.Web.Features.Board.Presentation.API.REST.Schemas;

internal sealed record CardUpdateRequest(
    Guid? ListId,
    string? Name,
    string? Description,
    CardPriority? Priority,
    DateTimeOffset? DueDate
);
