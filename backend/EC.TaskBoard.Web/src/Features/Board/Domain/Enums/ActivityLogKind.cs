namespace EC.TaskBoard.Web.Features.Board.Domain.Enums;

internal enum ActivityLogKind
{
    CardCreated,
    CardUpdatedName,
    CardUpdatedList,
    CardUpdatedDueDate,
    CardUpdatedPriority,
    CardUpdatedDescription,
    CardDeleted,
}
