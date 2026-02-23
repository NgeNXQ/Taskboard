using System;
using EC.TaskBoard.Web.Common.Foundation.Domain;

namespace EC.TaskBoard.Web.Features.Board.Domain.Guards;

internal static class CardGuard
{
    internal static void EnsureValidName(string? input, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new DomainException("Card name cannot be empty or whitespace");

        if (input.Length > maxLength)
            throw new DomainException($"Card Name exceeds {maxLength} characters");
    }

    internal static void EnsureValidDueDate(DateTimeOffset input)
    {
        if (input.Offset != TimeSpan.Zero)
            throw new DomainException("Card due date must be in UTC");

        if (input <= DateTimeOffset.UtcNow)
            throw new DomainException("Car due date must be a future date");
    }

    internal static void EnsureValidDescription(string? input, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new DomainException("Card description cannot be empty or whitespace");

        if (input.Length > maxLength)
            throw new DomainException($"Card Description exceeds {maxLength} characters");
    }
}
