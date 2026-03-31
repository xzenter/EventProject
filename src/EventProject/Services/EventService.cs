using EventProject.Controllers;
using EventProject.Controllers.Events.Query;
using EventProject.Controllers.Events.Response;
using EventProject.Exceptions;
using EventProject.Models;
using EventProject.Repository;

namespace EventProject.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _repository;

    public EventService(IEventRepository repository)
    {
        _repository = repository;
    }

    public PaginatedResult<EventDto> GetEvents(SearchEventsQuery query)
    {
        var filtered = _repository
            .GetAll()
            .Where(e =>
                (query.Title == null || e.Title.Contains(query.Title, StringComparison.OrdinalIgnoreCase)) &&
                (query.From == null || e.StartAt >= query.From) &&
                (query.To == null || e.EndAt <= query.To)
            );

        var filteredCount = filtered.Count();
        var items = filtered
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                StartAt = e.StartAt,
                EndAt = e.EndAt
            })
            .ToList();

        var totalPages = (int)Math.Ceiling((double)filteredCount / query.PageSize);

        return new PaginatedResult<EventDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = filteredCount,
            TotalPages = totalPages
        };
    }

    public EventDto GetEvent(Guid id)
    {
        var findEvent = _repository.GetById(id);

        if (findEvent == null)
        {
            throw new NotFoundException($"Событие с id = {id} не найдено");
        }

        return new EventDto
        {
            Id = findEvent.Id,
            Title = findEvent.Title,
            Description = findEvent.Description,
            StartAt = findEvent.StartAt,
            EndAt = findEvent.EndAt
        };
    }

    public EventDto CreateEvent(EventForCreationQuery eventForCreationQuery)
    {
        if (string.IsNullOrEmpty(eventForCreationQuery.Title))
        {
            throw new BadRequestException("Заголовок события не может быть пустым");
        }

        if (eventForCreationQuery.StartAt > eventForCreationQuery.EndAt)
        {
            throw new BadRequestException("Дата начала события не может быть позже даты окончания");
        }

        var newEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = eventForCreationQuery.Title,
            Description = eventForCreationQuery.Description,
            StartAt = eventForCreationQuery.StartAt,
            EndAt = eventForCreationQuery.EndAt
        };

        _repository.Add(newEvent);

        return new EventDto()
        {
            Id = newEvent.Id,
            Title = newEvent.Title,
            Description = newEvent.Description,
            StartAt = newEvent.StartAt,
            EndAt = newEvent.EndAt
        };
    }

    public EventDto UpdateEvent(Guid id, EventForUpdateQuery eventForUpdateQuery)
    {
        var findEvent = _repository.GetById(id);

        if (findEvent == null)
        {
            throw new NotFoundException($"Событие с id = {id} не найдено");
        }

        var newEvent = new Event
        {
            Id = id,
            Title = eventForUpdateQuery.Title,
            Description = eventForUpdateQuery.Description,
            StartAt = eventForUpdateQuery.StartAt,
            EndAt = eventForUpdateQuery.EndAt
        };

        _repository.Update(findEvent.Id, newEvent);

        return new EventDto()
        {
            Id = id,
            Title = eventForUpdateQuery.Title,
            Description = eventForUpdateQuery.Description,
            StartAt = eventForUpdateQuery.StartAt,
            EndAt = eventForUpdateQuery.EndAt
        };
    }

    public void DeleteEvent(Guid id)
    {
        _repository.Delete(id);

        var hasEvent = _repository.GetById(id) != null;

        if (hasEvent)
        {
            throw new Exception($"Событие с id = {id} не удалось удалить");
        }
    }
}