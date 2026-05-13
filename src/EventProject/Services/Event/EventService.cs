using EventProject.Dto;
using EventProject.Dto.Query;
using EventProject.Dto.Response;
using EventProject.Exceptions;
using EventProject.Repository.Event;

namespace EventProject.Services.Event;

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
                EndAt = e.EndAt,
                TotalSeats = e.TotalSeats,
                AvailableSeats = e.AvailableSeats
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
            EndAt = findEvent.EndAt,
            TotalSeats = findEvent.TotalSeats,
            AvailableSeats = findEvent.AvailableSeats
        };
    }

    public EventDto CreateEvent(EventForCreationQuery query)
    {
        if (string.IsNullOrEmpty(query.Title))
        {
            throw new BadRequestException("Заголовок события не может быть пустым");
        }

        if (query.StartAt > query.EndAt)
        {
            throw new BadRequestException("Дата начала события не может быть позже даты окончания");
        }

        var newEvent = new Models.Event
        {
            Id = Guid.NewGuid(),
            Title = query.Title,
            Description = query.Description,
            StartAt = query.StartAt,
            EndAt = query.EndAt,
            TotalSeats = query.TotalSeats,
            AvailableSeats = query.TotalSeats
        };

        _repository.Add(newEvent);

        return new EventDto()
        {
            Id = newEvent.Id,
            Title = newEvent.Title,
            Description = newEvent.Description,
            StartAt = newEvent.StartAt,
            EndAt = newEvent.EndAt,
            TotalSeats = newEvent.TotalSeats,
            AvailableSeats = newEvent.AvailableSeats 
        };
    }

    public EventDto UpdateEvent(Guid id, EventForUpdateQuery query)
    {
        if (query.StartAt > query.EndAt)
        {
            throw new BadRequestException("Дата начала события не может быть позже даты окончания");
        }

        var findEvent = _repository.GetById(id);

        if (findEvent == null)
        {
            throw new NotFoundException($"Событие с id = {id} не найдено");
        }

        var newEvent = new Models.Event
        {
            Id = id,
            Title = query.Title,
            Description = query.Description,
            StartAt = query.StartAt,
            EndAt = query.EndAt,
            TotalSeats = findEvent.TotalSeats,
            AvailableSeats = findEvent.AvailableSeats
        };

        _repository.Update(findEvent.Id, newEvent);

        return new EventDto()
        {
            Id = id,
            Title = query.Title,
            Description = query.Description,
            StartAt = query.StartAt,
            EndAt = query.EndAt
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