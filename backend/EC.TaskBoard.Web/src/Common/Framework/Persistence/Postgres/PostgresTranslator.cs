using System;
using System.Text.RegularExpressions;
using Npgsql;
using EC.TaskBoard.Web.Common.Framework.Persistence.Common;
using EC.TaskBoard.Web.Common.Foundation.Orchestration.Exceptions;

namespace EC.TaskBoard.Web.Common.Framework.Persistence.Postgres;

public sealed class PostgresTranslator : IDatabaseExceptionTranslator
{
    private const string ForeignKeyViolationCode = "23503";

    private static readonly Regex ForeignKeyViolationPattern = new Regex(
        @"Key \((?<key>\w+)\)=\((?<value>.+?)\) is not present in table ""(?<table>.+?)""",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    public void ThrowInnerException(Exception? exception)
    {
        if (exception is null)
            throw new ArgumentNullException(nameof(exception));

        if (exception is not PostgresException ex)
            throw new InvalidOperationException(nameof(exception));

        switch (ex.SqlState)
        {
            case ForeignKeyViolationCode:
                HandleForeignKeyViolationError(ex);
                break;
            default:
                throw new DatabaseException(ex.SqlState, ex.Message, ex);
        }
    }

    private void HandleForeignKeyViolationError(PostgresException exception)
    {
        string resourceIdentity = "UnknownEntity";
        string resourceIdentifier = "UnknownId";

        if (!string.IsNullOrEmpty(exception.Detail))
        {
            var match = ForeignKeyViolationPattern.Match(exception.Detail);

            if (match.Success)
            {
                resourceIdentity = match.Groups["table"].Value;
                resourceIdentifier = match.Groups["value"].Value;
            }
        }

        throw new EntryNotFoundException(resourceIdentity, resourceIdentifier);
    }
}
