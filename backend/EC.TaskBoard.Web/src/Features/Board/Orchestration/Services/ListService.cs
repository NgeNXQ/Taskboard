using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using AutoMapper;
using EC.TaskBoard.Web.Shared.Configs;
using EC.TaskBoard.Web.Common.Framework.Persistence;
using EC.TaskBoard.Web.Features.Board.Domain.Entities;
using EC.TaskBoard.Web.Features.Board.Orchestration.Models;
using EC.TaskBoard.Web.Features.Board.Orchestration.Interfaces;
using EC.TaskBoard.Web.Features.Board.Orchestration.Exceptions;

namespace EC.TaskBoard.Web.Features.Board.Orchestration.Services;

internal sealed class ListService : IListService
{
    private readonly IMapper _mapper;
    private readonly ListConfig _listConfig;
    private readonly IListRepository _listRepository;

    public ListService(
        IMapper mapper,
        IOptions<ListConfig> listConfig,
        IListRepository listRepository
    )
    {
        _mapper = mapper;
        _listConfig = listConfig.Value;
        _listRepository = listRepository;
    }

    public async Task<ListResult> CreateAsync(
        ListCreateParams parameters,
        CancellationToken token
    )
    {
        var list = List.Create(parameters.Name, _listConfig.MaxNameLength);

        await using (var session = await UnitOfWorkProxy.InstantiateAsync(_listRepository, token))
        {
            _listRepository.Create(list);

            await session.SaveAsync(token);
        }

        return _mapper.Map<ListResult>(list);
    }

    public async Task<IEnumerable<ListResult>> ReadAllAsync(CancellationToken token)
    {
        var lists = await _listRepository.ReadAllAsync(token);

        return _mapper.Map<IEnumerable<ListResult>>(lists);
    }

    public async Task<ListResult> UpdateAsync(
        ListUpdateParams parameters,
        CancellationToken token
    )
    {
        var list = await _listRepository.ReadByIdAsync(parameters.Id, token);

        if (list is null)
            throw new ListNotFoundException(parameters.Id);

        await using (var session = await UnitOfWorkProxy.InstantiateAsync(_listRepository, token))
        {
            list.ChangeName(parameters.Name, _listConfig.MaxNameLength);

            _listRepository.Update(list);

            await session.SaveAsync(token);
        }

        return _mapper.Map<ListResult>(list);
    }

    public async Task DeleteAsync(Guid id, CancellationToken token)
    {
        await using (var session = await UnitOfWorkProxy.InstantiateAsync(_listRepository, token))
        {
            var list = await _listRepository.ReadByIdAsync(id, token);

            if (list is null)
                return;

            _listRepository.Delete(list);

            await session.SaveAsync(token);
        }
    }

    public async Task<bool> DoesExistWitIdAsync(Guid id, CancellationToken token)
    {
        return await _listRepository.DoesExistWithIdAsync(id, token);
    }
}
