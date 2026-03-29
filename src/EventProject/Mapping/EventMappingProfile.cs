using AutoMapper;
using EventProject.Controllers.Events.Dto;
using EventProject.Models;

namespace EventProject.Mapping;

public class EventMappingProfile : Profile
{
    public EventMappingProfile()
    {
        CreateMap<Event, EventDto>().ReverseMap();
        CreateMap<EventForCreationDto, Event>();
    }
}