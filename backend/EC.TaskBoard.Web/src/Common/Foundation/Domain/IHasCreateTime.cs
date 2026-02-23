using System;

namespace EC.TaskBoard.Web.Common.Foundation.Domain;

public interface IHasCreateTime
{
    DateTime CreatedAt { get; set; }
}
