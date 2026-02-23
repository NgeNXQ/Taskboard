using EC.TaskBoard.Web.Common.Foundation.Domain;
using EC.TaskBoard.Web.Features.Board.Domain.Guards;
using Xunit;

namespace EC.TaskBoard.Web.Tests.Features.Board.Domain.Guards;

public sealed class ListGuardTests
{
    // -------------------------------------------------------------------------
    // EnsureValidName
    // -------------------------------------------------------------------------

    [Fact]
    public void EnsureValidName_ValidName_DoesNotThrow()
    {
        ListGuard.EnsureValidName("Backlog", 100);
    }

    [Fact]
    public void EnsureValidName_NameExactlyAtMaxLength_DoesNotThrow()
    {
        var name = new string('L', 50);

        ListGuard.EnsureValidName(name, 50);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EnsureValidName_NullOrWhitespace_ThrowsDomainException(string? name)
    {
        var exception = Assert.Throws<DomainException>(() =>
            ListGuard.EnsureValidName(name, 100));

        Assert.Equal("List name cannot be empty or whitespace", exception.Message);
    }

    [Fact]
    public void EnsureValidName_ExceedsMaxLength_ThrowsDomainExceptionWithLengthInfo()
    {
        const int maxLength = 10;
        var name = new string('L', maxLength + 1);

        var exception = Assert.Throws<DomainException>(() =>
            ListGuard.EnsureValidName(name, maxLength));

        Assert.Equal($"List Name exceeds {maxLength} characters", exception.Message);
    }
}
