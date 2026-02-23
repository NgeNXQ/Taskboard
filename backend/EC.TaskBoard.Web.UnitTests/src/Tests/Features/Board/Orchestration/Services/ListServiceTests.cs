using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using AutoMapper;
using EC.TaskBoard.Web.Shared.Configs;
using EC.TaskBoard.Web.Common.Framework.Persistence.Common;
using EC.TaskBoard.Web.Features.Board.Domain.Entities;
using EC.TaskBoard.Web.Features.Board.Orchestration.Models;
using EC.TaskBoard.Web.Features.Board.Orchestration.Services;
using EC.TaskBoard.Web.Features.Board.Orchestration.Mappings;
using EC.TaskBoard.Web.Features.Board.Orchestration.Exceptions;
using EC.TaskBoard.Web.Features.Board.Orchestration.Interfaces;

namespace EC.TaskBoard.Web.Tests.Features.Board.Orchestration.Services;

public sealed class ListServiceTests
{
    private const int MaxNameLength = 100;

    private readonly IMapper _mapper;
    private readonly Mock<IListRepository> _listRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ListService _sut;

    public ListServiceTests()
    {
        _mapper = new ServiceCollection()
            .AddLogging()
            .AddAutoMapper(cfg => cfg.AddProfile(new BoardProfile()))
            .BuildServiceProvider()
            .GetRequiredService<IMapper>();

        _listRepositoryMock = new Mock<IListRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _listRepositoryMock
            .Setup(r => r.InstantiateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        _sut = new ListService(
            _mapper,
            Options.Create(new ListConfig { MaxNameLength = MaxNameLength }),
            _listRepositoryMock.Object
        );
    }

    [Fact]
    public async Task CreateAsync_ValidParams_ReturnsListResult()
    {
        var parameters = new ListCreateParams("Backlog");

        var result = await _sut.CreateAsync(parameters, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Backlog", result.Name);
    }

    [Fact]
    public async Task CreateAsync_ValidParams_CallsRepositoryCreate()
    {
        var parameters = new ListCreateParams("Backlog");

        await _sut.CreateAsync(parameters, CancellationToken.None);

        _listRepositoryMock.Verify(r => r.Create(It.IsAny<List>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ValidParams_SavesUnitOfWork()
    {
        var parameters = new ListCreateParams("Backlog");

        await _sut.CreateAsync(parameters, CancellationToken.None);

        _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReadAllAsync_ReturnsMappedLists()
    {
        var lists = new[]
        {
            List.Create("Backlog", MaxNameLength),
            List.Create("In Progress", MaxNameLength),
        };

        _listRepositoryMock
            .Setup(r => r.ReadAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lists);

        var result = await _sut.ReadAllAsync(CancellationToken.None);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task ReadAllAsync_WhenEmpty_ReturnsEmptyCollection()
    {
        _listRepositoryMock
            .Setup(r => r.ReadAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<List>());

        var result = await _sut.ReadAllAsync(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateAsync_WhenListExists_UpdatesNameAndReturnsResult()
    {
        var list = List.Create("Old Name", MaxNameLength);
        var parameters = new ListUpdateParams(list.Id, "New Name");

        _listRepositoryMock
            .Setup(r => r.ReadByIdAsync(list.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var result = await _sut.UpdateAsync(parameters, CancellationToken.None);

        Assert.Equal("New Name", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_WhenListExists_CallsRepositoryUpdate()
    {
        var list = List.Create("Old Name", MaxNameLength);
        var parameters = new ListUpdateParams(list.Id, "New Name");

        _listRepositoryMock
            .Setup(r => r.ReadByIdAsync(list.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        await _sut.UpdateAsync(parameters, CancellationToken.None);

        _listRepositoryMock.Verify(r => r.Update(list), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenListExists_SavesUnitOfWork()
    {
        var list = List.Create("Old Name", MaxNameLength);
        var parameters = new ListUpdateParams(list.Id, "New Name");

        _listRepositoryMock
            .Setup(r => r.ReadByIdAsync(list.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        await _sut.UpdateAsync(parameters, CancellationToken.None);

        _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenListDoesNotExist_ThrowsListNotFoundException()
    {
        var id = Guid.NewGuid();
        var parameters = new ListUpdateParams(id, "New Name");

        _listRepositoryMock
            .Setup(r => r.ReadByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((List?)null);

        await Assert.ThrowsAsync<ListNotFoundException>(
            () => _sut.UpdateAsync(parameters, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_WhenListExists_CallsRepositoryDelete()
    {
        var list = List.Create("Backlog", MaxNameLength);

        _listRepositoryMock
            .Setup(r => r.ReadByIdAsync(list.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        await _sut.DeleteAsync(list.Id, CancellationToken.None);

        _listRepositoryMock.Verify(r => r.Delete(list), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenListExists_SavesUnitOfWork()
    {
        var list = List.Create("Backlog", MaxNameLength);

        _listRepositoryMock
            .Setup(r => r.ReadByIdAsync(list.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        await _sut.DeleteAsync(list.Id, CancellationToken.None);

        _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenListDoesNotExist_DoesNotCallDelete()
    {
        var id = Guid.NewGuid();

        _listRepositoryMock
            .Setup(r => r.ReadByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((List?)null);

        await _sut.DeleteAsync(id, CancellationToken.None);

        _listRepositoryMock.Verify(r => r.Delete(It.IsAny<List>()), Times.Never);
    }

    [Fact]
    public async Task DoesExistWitIdAsync_WhenListExists_ReturnsTrue()
    {
        var id = Guid.NewGuid();

        _listRepositoryMock
            .Setup(r => r.DoesExistWithIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.DoesExistWitIdAsync(id, CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task DoesExistWitIdAsync_WhenListDoesNotExist_ReturnsFalse()
    {
        var id = Guid.NewGuid();

        _listRepositoryMock
            .Setup(r => r.DoesExistWithIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.DoesExistWitIdAsync(id, CancellationToken.None);

        Assert.False(result);
    }
}
