using System;

namespace EC.TaskBoard.Web.Common.Foundation.Orchestration.Exceptions;

public class EntryNotFoundException : Exception
{
    public EntryNotFoundException(
        string resourceIdentity,
        string resourceIdentifier
    ) : base($"{resourceIdentifier} with {resourceIdentifier} does not exist")
    {
        ResourceIdentity = resourceIdentity;
        ResourceIdentifier = resourceIdentifier;
    }

    public string ResourceIdentity { get; }

    public string ResourceIdentifier { get; }
}
