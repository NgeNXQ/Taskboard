using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using AutoMapper;
using EC.TaskBoard.Web.Common.Framework.Persistence.Common;
using EC.TaskBoard.Web.Features.Board.Domain.Enums;
using EC.TaskBoard.Web.Features.Board.Domain.Entities;
using EC.TaskBoard.Web.Features.Board.Orchestration.Models;
using EC.TaskBoard.Web.Features.Board.Orchestration.Services;
using EC.TaskBoard.Web.Features.Board.Orchestration.Mappings;
using EC.TaskBoard.Web.Features.Board.Orchestration.Interfaces;

namespace EC.TaskBoard.Web.Tests.Features.Board.Orchestration.Services;

public sealed class ActivityLogServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IActivityLogRepository> _activityLogRepositoryMock;
    private readonly IMapper _mapper;
    private readonly ActivityLogService _sut;

    public ActivityLogServiceTests()
    {
        _mapper = new ServiceCollection()
            .AddLogging()
            .AddAutoMapper(cfg => cfg.AddProfile(new BoardProfile()))
            .BuildServiceProvider()
            .GetRequiredService<IMapper>();

        _activityLogRepositoryMock = new Mock<IActivityLogRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _activityLogRepositoryMock
            .Setup(r => r.InstantiateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        _sut = new ActivityLogService(_mapper, _activityLogRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidParams_CallsRepositoryCreate()
    {
        var parameters = new ActivityLogCreateParams(
            Guid.NewGuid(),
            ActivityLogKind.CardCreated,
            "Current Name",
            null
        );

        await _sut.CreateAsync(parameters, CancellationToken.None);

        _activityLogRepositoryMock.Verify(r => r.Create(It.IsAny<ActivityLog>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithNullCurrentAndPrevious_CallsRepositoryCreate()
    {
        var parameters = new ActivityLogCreateParams(
            Guid.NewGuid(),
            ActivityLogKind.CardDeleted,
            null,
            null
        );

        await _sut.CreateAsync(parameters, CancellationToken.None);

        _activityLogRepositoryMock.Verify(r => r.Create(It.IsAny<ActivityLog>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DoesNotSaveUnitOfWork()
    {
        var parameters = new ActivityLogCreateParams(
            Guid.NewGuid(),
            ActivityLogKind.CardCreated,
            "Name",
            null
        );

        await _sut.CreateAsync(parameters, CancellationToken.None);

        _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReadAllByCardIdAsync_ReturnsMappedResults()
    {
        var cardId = Guid.NewGuid();
        var logs = new[]
        {
            ActivityLog.Create(cardId, ActivityLogKind.CardCreated, "Name", null),
            ActivityLog.Create(cardId, ActivityLogKind.CardUpdatedName, "New", "Old"),
        };

        _activityLogRepositoryMock
            .Setup(r => r.ReadAllByCardIdAsync(cardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);

        var result = await _sut.ReadAllByCardIdAsync(cardId, CancellationToken.None);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task ReadAllByCardIdAsync_WhenNoLogs_ReturnsEmpty()
    {
        var cardId = Guid.NewGuid();

        _activityLogRepositoryMock
            .Setup(r => r.ReadAllByCardIdAsync(cardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ActivityLog>());

        var result = await _sut.ReadAllByCardIdAsync(cardId, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ReadAllPaginatedAsync_ReturnsMappedDataWithCorrectMeta()
    {
        var logs = new[]
        {
            ActivityLog.Create(Guid.NewGuid(), ActivityLogKind.CardCreated, "Name", null),
        };

        _activityLogRepositoryMock
            .Setup(r => r.ReadAllPaginatedAsync(0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((logs, 25));

        var result = await _sut.ReadAllPaginatedAsync(0, 10, CancellationToken.None);

        Assert.Single(result.Data);
        Assert.Equal(0, result.Meta.Offset);
        Assert.Equal(10, result.Meta.Limit);
        Assert.Equal(25, result.Meta.Total);
        Assert.True(result.Meta.HasMore);
    }

    [Fact]
    public async Task ReadAllPaginatedAsync_WhenLastPage_HasMoreIsFalse()
    {
        var logs = new[]
        {
            ActivityLog.Create(Guid.NewGuid(), ActivityLogKind.CardCreated, "Name", null),
        };

        // offset=20, limit=10, total=25 → offset + limit = 30 >= 25 → HasMore = false
        _activityLogRepositoryMock
            .Setup(r => r.ReadAllPaginatedAsync(20, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((logs, 25));

        var result = await _sut.ReadAllPaginatedAsync(20, 10, CancellationToken.None);

        Assert.False(result.Meta.HasMore);
    }

    [Fact]
    public async Task ReadAllPaginatedAsync_WhenNoData_ReturnsEmptyDataAndZeroTotal()
    {
        _activityLogRepositoryMock
            .Setup(r => r.ReadAllPaginatedAsync(0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enumerable.Empty<ActivityLog>(), 0));

        var result = await _sut.ReadAllPaginatedAsync(0, 10, CancellationToken.None);

        Assert.Empty(result.Data);
        Assert.Equal(0, result.Meta.Total);
        Assert.False(result.Meta.HasMore);
    }
}
