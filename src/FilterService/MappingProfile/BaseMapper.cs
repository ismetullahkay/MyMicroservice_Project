using AutoMapper;
using Contracts;
using FilterService.Consumer;
using FilterService.Models;

namespace FilterService.MappingProfile;

public class BaseMapper : Profile
{
    public BaseMapper()
    {
        CreateMap<GameFilterItem,GameCreated>().ReverseMap();
    }

    
}