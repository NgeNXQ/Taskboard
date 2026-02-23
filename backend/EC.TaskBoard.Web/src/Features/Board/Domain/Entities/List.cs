using System;
using EC.TaskBoard.Web.Common.Foundation.Domain;
using EC.TaskBoard.Web.Features.Board.Domain.Guards;

namespace EC.TaskBoard.Web.Features.Board.Domain.Entities;

internal sealed class List : Entity<Guid>, IHasCreateTime, IHasUpdateTime
{
    internal static List Create(string name, int maxNameLength)
    {
        ListGuard.EnsureValidName(name, maxNameLength);

        return new List()
        {
            Name = name
        };
    }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string Name { get; private set; } = string.Empty;

    internal void ChangeName(string name, int maxLength)
    {
        ListGuard.EnsureValidName(name, maxLength);

        Name = name;
    }
}
