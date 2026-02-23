using System;

namespace EC.TaskBoard.Web.Common.Framework.Persistence.Common;

public sealed class DatabaseException : Exception
{
    public string Code { get; }

    public DatabaseException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }
}
