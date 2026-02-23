using System;
using MediatR;

namespace EC.TaskBoard.Web.Features.Board.Orchestration.Notifications;

internal sealed record CardDeletedNotification(
    Guid CardId,
    string Name
) : INotification;
