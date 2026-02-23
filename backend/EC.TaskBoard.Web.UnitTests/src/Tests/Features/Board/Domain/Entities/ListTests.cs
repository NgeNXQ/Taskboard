using Xunit;
using EC.TaskBoard.Web.Common.Foundation.Domain;
using EC.TaskBoard.Web.Features.Board.Domain.Entities;

namespace EC.TaskBoard.Web.Tests.Features.Board.Domain.Entities;

public sealed class ListTests
{
    private const int MaxNameLength = 200;

    [Fact]
    public void Create_ValidName_ReturnsListWithName()
    {
        var list = List.Create("Backlog", MaxNameLength);

        Assert.NotNull(list);
        Assert.Equal("Backlog", list.Name);
    }

    [Fact]
    public void Create_ValidName_ListIsTransient()
    {
        var list = List.Create("Backlog", MaxNameLength);

        Assert.True(list.IsTransient);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyName_ThrowsDomainException(string? name)
    {
        Assert.Throws<DomainException>(() => List.Create(name!, MaxNameLength));
    }

    [Fact]
    public void Create_NameExceedsMaxLength_ThrowsDomainException()
    {
        var longName = new string('L', MaxNameLength + 1);

        Assert.Throws<DomainException>(() => List.Create(longName, MaxNameLength));
    }

    [Fact]
    public void Create_NameExactlyAtMaxLength_DoesNotThrow()
    {
        var name = new string('L', MaxNameLength);

        var list = List.Create(name, MaxNameLength);

        Assert.Equal(name, list.Name);
    }

    [Fact]
    public void ChangeName_ValidName_UpdatesName()
    {
        var list = List.Create("Old Name", MaxNameLength);

        list.ChangeName("New Name", MaxNameLength);

        Assert.Equal("New Name", list.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangeName_EmptyName_ThrowsDomainException(string? name)
    {
        var list = List.Create("Backlog", MaxNameLength);

        Assert.Throws<DomainException>(() => list.ChangeName(name!, MaxNameLength));
    }

    [Fact]
    public void ChangeName_NameExceedsMaxLength_ThrowsDomainException()
    {
        var list = List.Create("Backlog", MaxNameLength);
        var longName = new string('L', MaxNameLength + 1);

        Assert.Throws<DomainException>(() => list.ChangeName(longName, MaxNameLength));
    }
}
