using EC.TaskBoard.Web.Common.Foundation.Domain;

namespace EC.TaskBoard.Web.Features.Board.Domain.Guards;

internal static class ListGuard
{
    internal static void EnsureValidName(string? input, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new DomainException("List name cannot be empty or whitespace");

        if (input.Length > maxLength)
            throw new DomainException($"List Name exceeds {maxLength} characters");
    }
}
