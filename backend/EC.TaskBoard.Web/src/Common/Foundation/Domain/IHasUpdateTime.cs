using System;

namespace EC.TaskBoard.Web.Common.Foundation.Domain;

public interface IHasUpdateTime
{
    DateTime UpdatedAt { get; internal set; }
}
