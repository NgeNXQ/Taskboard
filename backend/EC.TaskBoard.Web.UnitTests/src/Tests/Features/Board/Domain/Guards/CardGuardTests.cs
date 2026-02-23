using System;
using Xunit;
using EC.TaskBoard.Web.Common.Foundation.Domain;
using EC.TaskBoard.Web.Features.Board.Domain.Guards;

namespace EC.TaskBoard.Web.Tests.Features.Board.Domain.Guards;

public sealed class CardGuardTests
{
    private const int MaxNameLength = 100;
    private const int MaxDescriptionLength = 2000;

    [Fact]
    public void EnsureValidName_ValidName_DoesNotThrow()
    {
        CardGuard.EnsureValidName("My Card", MaxNameLength);
    }

    [Fact]
    public void EnsureValidName_NameAtMaxLength_DoesNotThrow()
    {
        var name = new string('a', MaxNameLength);

        CardGuard.EnsureValidName(name, MaxNameLength);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EnsureValidName_NullOrWhitespace_ThrowsDomainException(string? name)
    {
        var exception = Assert.Throws<DomainException>(() =>
            CardGuard.EnsureValidName(name, MaxNameLength));

        Assert.Equal("Card name cannot be empty or whitespace", exception.Message);
    }

    [Fact]
    public void EnsureValidName_NameExceedsMaxLength_ThrowsDomainExceptionWithLengthInfo()
    {
        var name = new string('a', MaxNameLength + 1);

        var exception = Assert.Throws<DomainException>(() =>
            CardGuard.EnsureValidName(name, MaxNameLength));

        Assert.Equal($"Card Name exceeds {MaxNameLength} characters", exception.Message);
    }

    [Fact]
    public void EnsureValidDueDate_FutureDateInUtc_DoesNotThrow()
    {
        var futureDate = DateTimeOffset.UtcNow.AddDays(1);

        CardGuard.EnsureValidDueDate(futureDate);
    }

    [Fact]
    public void EnsureValidDueDate_NonUtcOffset_ThrowsDomainException()
    {
        TimeSpan offset = TimeSpan.FromHours(2);

        var nonUtdDate = new DateTimeOffset(
            DateTime.Now.AddYears(1).Year,
            DateTime.Now.Month,
            DateTime.Now.Day,
            12, 0, 0,
            offset
        );

        var exception = Assert.Throws<DomainException>(() =>
            CardGuard.EnsureValidDueDate(nonUtdDate));

        Assert.Equal("Card due date must be in UTC", exception.Message);
    }

    [Fact]
    public void EnsureValidDueDate_PastDate_ThrowsDomainException()
    {
        var pastDate = DateTimeOffset.UtcNow.AddDays(-1);

        var exception = Assert.Throws<DomainException>(() =>
            CardGuard.EnsureValidDueDate(pastDate));

        Assert.Equal("Car due date must be a future date", exception.Message);
    }

    [Fact]
    public void EnsureValidDueDate_UtcNow_ThrowsDomainException()
    {
        var now = DateTimeOffset.UtcNow;

        Assert.Throws<DomainException>(() =>
            CardGuard.EnsureValidDueDate(now));
    }

    [Fact]
    public void EnsureValidDescription_ValidDescription_DoesNotThrow()
    {
        CardGuard.EnsureValidDescription("A valid description", MaxDescriptionLength);
    }

    [Fact]
    public void EnsureValidDescription_DescriptionAtMaxLength_DoesNotThrow()
    {
        var description = new string('x', MaxDescriptionLength);

        CardGuard.EnsureValidDescription(description, MaxDescriptionLength);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EnsureValidDescription_NullOrWhitespace_ThrowsDomainException(string? description)
    {
        var exception = Assert.Throws<DomainException>(() =>
            CardGuard.EnsureValidDescription(description, MaxDescriptionLength));

        Assert.Equal("Card description cannot be empty or whitespace", exception.Message);
    }

    [Fact]
    public void EnsureValidDescription_ExceedsMaxLength_ThrowsDomainExceptionWithLengthInfo()
    {
        var description = new string('d', MaxDescriptionLength + 1);

        var exception = Assert.Throws<DomainException>(() =>
            CardGuard.EnsureValidDescription(description, MaxDescriptionLength));

        Assert.Equal(
            $"Card Description exceeds {MaxDescriptionLength} characters", exception.Message
        );
    }
}
