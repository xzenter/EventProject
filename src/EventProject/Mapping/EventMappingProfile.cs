using AutoMapper;
using EventProject.Controllers.Events.Query;
using EventProject.Controllers.Events.Response;
using EventProject.Models;

namespace EventProject.Mapping;

public class EventMappingProfile : Profile
{
    public EventMappingProfile()
    {
        CreateMap<Event, EventDto>().ReverseMap();
        CreateMap<EventForCreationQuery, Event>();
    }
}