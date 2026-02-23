using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using MediatR;
using AutoMapper;
using EC.TaskBoard.Web.Shared.Configs;
using EC.TaskBoard.Web.Common.Framework.Persistence;
using EC.TaskBoard.Web.Features.Board.Domain.Entities;
using EC.TaskBoard.Web.Features.Board.Orchestration.Models;
using EC.TaskBoard.Web.Features.Board.Orchestration.Interfaces;
using EC.TaskBoard.Web.Features.Board.Orchestration.Exceptions;
using EC.TaskBoard.Web.Features.Board.Orchestration.Notifications;

namespace EC.TaskBoard.Web.Features.Board.Orchestration.Services;

internal sealed class CardService : ICardService
{
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;
    private readonly CardConfig _cardConfig;
    private readonly IListService _listService;
    private readonly ICardRepository _cardRepository;

    public CardService(
        IMapper mapper,
        IOptions<CardConfig> cardConfig,
        IPublisher publisher,
        IListService listService,
        ICardRepository cardRepository
    )
    {
        _mapper = mapper;
        _publisher = publisher;
        _cardConfig = cardConfig.Value;
        _listService = listService;
        _cardRepository = cardRepository;
    }

    public async Task<CardResult> CreateAsync(CardCreateParams parameters, CancellationToken token)
    {
        var doesListExist = await _listService.DoesExistWitIdAsync(parameters.ListId, token);

        if (!doesListExist)
            throw new ListNotFoundException(parameters.ListId);

        var card = Card.Create(
            parameters.ListId,
            parameters.Name,
            parameters.Priority,
            parameters.Description,
            parameters.DueDate,
            _cardConfig.MaxNameLength,
            _cardConfig.MaxDescriptionLength
        );

        await using (var session = await UnitOfWorkProxy.InstantiateAsync(_cardRepository, token))
        {
            _cardRepository.Create(card);

            await _publisher.Publish(new CardCreatedNotification(card.Id, card.Name), token);

            await session.SaveAsync(token);
        }

        return _mapper.Map<CardResult>(card);
    }

    public async Task<CardResult> ReadByIdAsync(Guid id, CancellationToken token)
    {
        var card = await _cardRepository.ReadByIdAsync(id, token);

        if (card is null)
            throw new CardNotFoundException(id);

        return _mapper.Map<CardResult>(card);
    }

    public async Task<IEnumerable<CardResult>> ReadAllAsync(CancellationToken token)
    {
        var cards = await _cardRepository.ReadAllAsync(token);

        return _mapper.Map<IEnumerable<CardResult>>(cards);
    }

    public async Task<CardResult> UpdatePartiallyAsync(
        CardUpdatePartialParams parameters,
        CancellationToken token
    )
    {
        var card = await _cardRepository.ReadByIdAsync(parameters.Id, token);

        if (card is null)
            throw new CardNotFoundException(parameters.Id);

        await using (var session = await UnitOfWorkProxy.InstantiateAsync(_cardRepository, token))
        {
            if (parameters.Name is not null)
                card.ChangeName(parameters.Name, _cardConfig.MaxNameLength);

            if (parameters.DueDate is not null)
                card.ChangeDueDate(parameters.DueDate.Value);

            if (parameters.Priority is not null)
                card.ChangePriority(parameters.Priority.Value);

            if (parameters.Description is not null)
                card.ChangeDescription(parameters.Description, _cardConfig.MaxDescriptionLength);

            if (parameters.ListId is not null)
            {
                var listId = parameters.ListId.Value;
                var doesListExist = await _listService.DoesExistWitIdAsync(listId, token);

                if (!doesListExist)
                    throw new ListNotFoundException(listId);

                card.ChangeList(listId);
            }

            _cardRepository.Update(card);

            await session.SaveAsync(token);
        }

        return _mapper.Map<CardResult>(card);
    }

    public async Task DeleteAsync(Guid id, CancellationToken token)
    {
        await using (var session = await UnitOfWorkProxy.InstantiateAsync(_cardRepository, token))
        {
            var card = await _cardRepository.ReadByIdAsync(id, token);

            if (card is null)
                return;

            _cardRepository.Delete(card);

            await _publisher.Publish(new CardDeletedNotification(card.Id, card.Name), token);

            await session.SaveAsync(token);
        }
    }
}
