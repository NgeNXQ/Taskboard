using System;

namespace EC.TaskBoard.Web.Common.Foundation.Domain;

public interface IHasDeleteTime
{
    DateTime? DeletedAt { get; internal set; }
}
