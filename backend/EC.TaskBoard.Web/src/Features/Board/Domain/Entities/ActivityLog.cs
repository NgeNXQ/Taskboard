using System;
using EC.TaskBoard.Web.Common.Foundation.Domain;
using EC.TaskBoard.Web.Features.Board.Domain.Enums;

namespace EC.TaskBoard.Web.Features.Board.Domain.Entities;

internal sealed class ActivityLog : Entity<Guid>, IHasCreateTime
{
    internal static ActivityLog Create(
        Guid cardId,
        ActivityLogKind kind,
        string? current,
        string? previous
    )
    {
        return new ActivityLog()
        {
            CardId = cardId,
            Kind = kind,
            Current = current,
            Previous = previous
        };
    }

    public DateTime CreatedAt { get; set; }

    public Guid? CardId { get; private set; }
    public ActivityLogKind Kind { get; private set; }
    public string? Current { get; private set; } = string.Empty;
    public string? Previous { get; private set; } = string.Empty;
}
