using AutoMapper;
using EC.TaskBoard.Web.Features.Board.Domain.Entities;
using EC.TaskBoard.Web.Features.Board.Orchestration.Models;

namespace EC.TaskBoard.Web.Features.Board.Orchestration.Mappings;

internal sealed class BoardProfile : Profile
{
    internal BoardProfile()
    {
        CreateMap<Card, CardResult>();

        CreateMap<List, ListResult>();

        CreateMap<ActivityLog, ActivityLogResult>();
    }
}
