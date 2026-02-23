using System;

namespace EC.TaskBoard.Web.Common.Foundation.Domain;

public class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }
}
