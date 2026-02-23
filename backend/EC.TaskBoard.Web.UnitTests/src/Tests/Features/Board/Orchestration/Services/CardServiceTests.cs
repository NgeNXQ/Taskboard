using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using MediatR;
using AutoMapper;
using EC.TaskBoard.Web.Shared.Configs;
using EC.TaskBoard.Web.Common.Framework.Persistence.Common;
using EC.TaskBoard.Web.Features.Board.Domain.Enums;
using EC.TaskBoard.Web.Features.Board.Domain.Entities;
using EC.TaskBoard.Web.Features.Board.Orchestration.Models;
using EC.TaskBoard.Web.Features.Board.Orchestration.Services;
using EC.TaskBoard.Web.Features.Board.Orchestration.Mappings;
using EC.TaskBoard.Web.Features.Board.Orchestration.Exceptions;
using EC.TaskBoard.Web.Features.Board.Orchestration.Interfaces;
using EC.TaskBoard.Web.Features.Board.Orchestration.Notifications;

namespace EC.TaskBoard.Web.Tests.Features.Board.Orchestration.Services;

public sealed class CardServiceTests
{
    private const int MaxNameLength = 200;
    private const int MaxDescriptionLength = 2000;

    private readonly IMapper _mapper;
    private readonly Mock<IPublisher> _publisherMock;
    private readonly Mock<IListService> _listServiceMock;
    private readonly Mock<ICardRepository> _cardRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CardService _sut;

    public CardServiceTests()
    {
        _mapper = new ServiceCollection()
            .AddLogging()
            .AddAutoMapper(cfg => cfg.AddProfile(new BoardProfile()))
            .BuildServiceProvider()
            .GetRequiredService<IMapper>();

        _publisherMock = new Mock<IPublisher>();
        _listServiceMock = new Mock<IListService>();
        _cardRepositoryMock = new Mock<ICardRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _cardRepositoryMock
            .Setup(r => r.InstantiateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWorkMock.Object);

        var cardConfig = Options.Create(new CardConfig
        {
            MaxNameLength = MaxNameLength,
            MaxDescriptionLength = MaxDescriptionLength,
        });

        _sut = new CardService(
            _mapper,
            cardConfig,
            _publisherMock.Object,
            _listServiceMock.Object,
            _cardRepositoryMock.Object
        );
    }

    [Fact]
    public async Task CreateAsync_WhenListExists_CreatesAndReturnsCard()
    {
        var listId = Guid.NewGuid();
        var parameters = ValidCreateParams(listId);

        _listServiceMock
            .Setup(s => s.DoesExistWitIdAsync(listId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.CreateAsync(parameters, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(parameters.Name, result.Name);
        Assert.Equal(parameters.Description, result.Description);
        Assert.Equal(parameters.Priority, result.Priority);
        Assert.Equal(parameters.DueDate, result.DueDate);
        Assert.Equal(listId, result.ListId);
    }

    [Fact]
    public async Task CreateAsync_WhenListExists_CallsRepositoryCreate()
    {
        var listId = Guid.NewGuid();
        var parameters = ValidCreateParams(listId);

        _listServiceMock
            .Setup(s => s.DoesExistWitIdAsync(listId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _sut.CreateAsync(parameters, CancellationToken.None);

        _cardRepositoryMock.Verify(r => r.Create(It.IsAny<Card>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenListExists_SavesUnitOfWork()
    {
        var listId = Guid.NewGuid();
        var parameters = ValidCreateParams(listId);

        _listServiceMock
            .Setup(s => s.DoesExistWitIdAsync(listId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _sut.CreateAsync(parameters, CancellationToken.None);

        _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenListExists_PublishesCardCreatedNotification()
    {
        var listId = Guid.NewGuid();
        var parameters = ValidCreateParams(listId);

        _listServiceMock
            .Setup(s => s.DoesExistWitIdAsync(listId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _sut.CreateAsync(parameters, CancellationToken.None);

        _publisherMock.Verify(
            p => p.Publish(
                It.Is<CardCreatedNotification>(n => n.Name == parameters.Name),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateAsync_WhenListDoesNotExist_ThrowsListNotFoundException()
    {
        var listId = Guid.NewGuid();
        var parameters = ValidCreateParams(listId);

        _listServiceMock
            .Setup(s => s.DoesExistWitIdAsync(listId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<ListNotFoundException>(
            () => _sut.CreateAsync(parameters, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WhenListDoesNotExist_DoesNotCallRepositoryCreate()
    {
        var listId = Guid.NewGuid();
        var parameters = ValidCreateParams(listId);

        _listServiceMock
            .Setup(s => s.DoesExistWitIdAsync(listId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<ListNotFoundException>(
            () => _sut.CreateAsync(parameters, CancellationToken.None));

        _cardRepositoryMock.Verify(r => r.Create(It.IsAny<Card>()), Times.Never);
    }

    [Fact]
    public async Task ReadByIdAsync_WhenCardExists_ReturnsCardResult()
    {
        var card = CreateCard();
        var id = card.Id;

        _cardRepositoryMock
            .Setup(r => r.ReadByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        var result = await _sut.ReadByIdAsync(id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(card.Name, result.Name);
    }

    [Fact]
    public async Task ReadByIdAsync_WhenCardDoesNotExist_ThrowsCardNotFoundException()
    {
        var id = Guid.NewGuid();

        _cardRepositoryMock
            .Setup(r => r.ReadByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Card?)null);

        await Assert.ThrowsAsync<CardNotFoundException>(
            () => _sut.ReadByIdAsync(id, CancellationToken.None));
    }

    [Fact]
    public async Task ReadAllAsync_ReturnsMappedCards()
    {
        var cards = new[] { CreateCard(), CreateCard() };

        _cardRepositoryMock
            .Setup(r => r.ReadAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(cards);

        var result = await _sut.ReadAllAsync(CancellationToken.None);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task ReadAllAsync_WhenNoCards_ReturnsEmpty()
    {
        _cardRepositoryMock
            .Setup(r => r.ReadAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Card>());

        var result = await _sut.ReadAllAsync(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdatePartiallyAsync_WhenCardDoesNotExist_ThrowsCardNotFoundException()
    {
        var id = Guid.NewGuid();
        var parameters = new CardUpdatePartialParams(id, null, null, null, null, null);

        _cardRepositoryMock
            .Setup(r => r.ReadByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Card?)null);

        await Assert.ThrowsAsync<CardNotFoundException>(
            () => _sut.UpdatePartiallyAsync(parameters, CancellationToken.None));
    }

    [Fact]
    public async Task UpdatePartiallyAsync_WhenNameProvided_UpdatesName()
    {
        var card = CreateCard();
        var parameters = new CardUpdatePartialParams(card.Id, null, "Updated Name", null, null, null);

        _cardRepositoryMock
            .Setup(r => r.ReadByIdAsync(card.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        var result = await _sut.UpdatePartiallyAsync(parameters, CancellationToken.None);

        Assert.Equal("Updated Name", result.Name);
    }

    [Fact]
    public async Task UpdatePartiallyAsync_WhenPriorityProvided_UpdatesPriority()
    {
        var card = CreateCard();
        var parameters = new CardUpdatePartialParams(card.Id, null, null, null, CardPriority.Highest, null);

        _cardRepositoryMock
            .Setup(r => r.ReadByIdAsync(card.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        var result = await _sut.UpdatePartiallyAsync(parameters, CancellationToken.None);

        Assert.Equal(CardPriority.Highest, result.Priority);
    }

    [Fact]
    public async Task UpdatePartiallyAsync_WhenDueDateProvided_UpdatesDueDate()
    {
        var card = CreateCard();
        var newDate = DateTimeOffset.UtcNow.AddDays(30);
        var parameters = new CardUpdatePartialParams(card.Id, null, null, null, null, newDate);

        _cardRepositoryMock
            .Setup(r => r.ReadByIdAsync(card.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        var result = await _sut.UpdatePartiallyAsync(parameters, CancellationToken.None);

        Assert.Equal(newDate, result.DueDate);
    }

    [Fact]
    public async Task UpdatePartiallyAsync_WhenDescriptionProvided_UpdatesDescription()
    {
        var card = CreateCard();
        var parameters = new CardUpdatePartialParams(card.Id, null, null, "New description", null, null);

        _cardRepositoryMock
            .Setup(r => r.ReadByIdAsync(card.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        var result = await _sut.UpdatePartiallyAsync(parameters, CancellationToken.None);

        Assert.Equal("New description", result.Description);
    }

    [Fact]
    public async Task UpdatePartiallyAsync_WhenListIdProvidedAndListExists_UpdatesListId()
    {
        var card = CreateCard();
        var newListId = Guid.NewGuid();
        var parameters = new CardUpdatePartialParams(card.Id, newListId, null, null, null, null);

        _cardRepositoryMock
            .Setup(r => r.ReadByIdAsync(card.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        _listServiceMock
            .Setup(s => s.DoesExistWitIdAsync(newListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.UpdatePartiallyAsync(parameters, CancellationToken.None);

        Assert.Equal(newListId, result.ListId);
    }

    [Fact]
    public async Task UpdatePartiallyAsync_WhenListIdProvidedAndListDoesNotExist_ThrowsListNotFoundException()
    {
        var card = CreateCard();
        var newListId = Guid.NewGuid();
        var parameters = new CardUpdatePartialParams(card.Id, newListId, null, null, null, null);

        _cardRepositoryMock
            .Setup(r => r.ReadByIdAsync(card.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        _listServiceMock
            .Setup(s => s.DoesExistWitIdAsync(newListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<ListNotFoundException>(
            () => _sut.UpdatePartiallyAsync(parameters, CancellationToken.None));
    }

    [Fact]
    public async Task UpdatePartiallyAsync_WhenNullParametersProvided_DoesNotChangeCard()
    {
        var card = CreateCard();
        var originalName = card.Name;
        var parameters = new CardUpdatePartialParams(card.Id, null, null, null, null, null);

        _cardRepositoryMock
            .Setup(r => r.ReadByIdAsync(card.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        var result = await _sut.UpdatePartiallyAsync(parameters, CancellationToken.None);

        Assert.Equal(originalName, result.Name);
    }

    [Fact]
    public async Task UpdatePartiallyAsync_WhenCardExists_CallsRepositoryUpdate()
    {
        var card = CreateCard();
        var parameters = new CardUpdatePartialParams(card.Id, null, null, null, null, null);

        _cardRepositoryMock
            .Setup(r => r.ReadByIdAsync(card.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        await _sut.UpdatePartiallyAsync(parameters, CancellationToken.None);

        _cardRepositoryMock.Verify(r => r.Update(card), Times.Once);
    }

    [Fact]
    public async Task UpdatePartiallyAsync_WhenCardExists_SavesUnitOfWork()
    {
        var card = CreateCard();
        var parameters = new CardUpdatePartialParams(card.Id, null, null, null, null, null);

        _cardRepositoryMock
            .Setup(r => r.ReadByIdAsync(card.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        await _sut.UpdatePartiallyAsync(parameters, CancellationToken.None);

        _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenCardExists_CallsRepositoryDelete()
    {
        var card = CreateCard();

        _cardRepositoryMock
            .Setup(r => r.ReadByIdAsync(card.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        await _sut.DeleteAsync(card.Id, CancellationToken.None);

        _cardRepositoryMock.Verify(r => r.Delete(card), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenCardExists_PublishesCardDeletedNotification()
    {
        var card = CreateCard();

        _cardRepositoryMock
            .Setup(r => r.ReadByIdAsync(card.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        await _sut.DeleteAsync(card.Id, CancellationToken.None);

        _publisherMock.Verify(
            p => p.Publish(
                It.Is<CardDeletedNotification>(n => n.Name == card.Name),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteAsync_WhenCardExists_SavesUnitOfWork()
    {
        var card = CreateCard();

        _cardRepositoryMock
            .Setup(r => r.ReadByIdAsync(card.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        await _sut.DeleteAsync(card.Id, CancellationToken.None);

        _unitOfWorkMock.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenCardDoesNotExist_DoesNotCallDelete()
    {
        var id = Guid.NewGuid();

        _cardRepositoryMock
            .Setup(r => r.ReadByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Card?)null);

        await _sut.DeleteAsync(id, CancellationToken.None);

        _cardRepositoryMock.Verify(r => r.Delete(It.IsAny<Card>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenCardDoesNotExist_DoesNotPublishNotification()
    {
        var id = Guid.NewGuid();

        _cardRepositoryMock
            .Setup(r => r.ReadByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Card?)null);

        await _sut.DeleteAsync(id, CancellationToken.None);

        _publisherMock.Verify(
            p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    private static CardCreateParams ValidCreateParams(Guid? listId = null) =>
        new(
            ListId: listId ?? Guid.NewGuid(),
            Name: "Test Card",
            Description: "Test description",
            Priority: CardPriority.Medium,
            DueDate: DateTimeOffset.UtcNow.AddDays(7)
        );

    private static Card CreateCard() =>
        Card.Create(
            Guid.NewGuid(),
            "Test Card",
            CardPriority.Medium,
            "Test description",
            DateTimeOffset.UtcNow.AddDays(7),
            MaxNameLength,
            MaxDescriptionLength
        );
}