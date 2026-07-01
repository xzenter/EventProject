using EventProject.Application.Abstractions.Repositories;
using EventProject.Application.Abstractions.Services;
using EventProject.Application.DTOs;
using EventProject.Application.Events.DTOs;
using EventProject.Domain.Exceptions;

namespace EventProject.Application.Events;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;

    public EventService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<PaginatedResult<EventDto>> GetEvents(SearchEventsQuery query, CancellationToken ct = default)
    {
        var events = await _eventRepository.GetByFilter(query.Title, query.From, query.To, ct);
        var filtered = events.ToList();

        var filteredCount = filtered.Count;
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

    public async Task<EventDto> GetEvent(Guid id, CancellationToken ct = default)
    {
        var findEvent = await _eventRepository.GetById(id, ct);

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

    public async Task<EventDto> CreateEvent(EventForCreationQuery query, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(query.Title))
        {
            throw new BadRequestException("Заголовок события не может быть пустым");
        }

        if (query.StartAt > query.EndAt)
        {
            throw new BadRequestException("Дата начала события не может быть позже даты окончания");
        }

        var newEvent = new Domain.Entities.Event
        {
            Id = Guid.NewGuid(),
            Title = query.Title,
            Description = query.Description,
            StartAt = query.StartAt,
            EndAt = query.EndAt,
            TotalSeats = query.TotalSeats,
            AvailableSeats = query.TotalSeats
        };

        await _eventRepository.Add(newEvent, ct);
        await _eventRepository.SaveChanges(ct);

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

    public async Task<EventDto> UpdateEvent(Guid id, EventForUpdateQuery query, CancellationToken ct = default)
    {
        if (query.StartAt > query.EndAt)
        {
            throw new BadRequestException("Дата начала события не может быть позже даты окончания");
        }

        var findEvent = await _eventRepository.GetById(id, ct);

        if (findEvent == null)
        {
            throw new NotFoundException($"Событие с id = {id} не найдено");
        }

        findEvent.Title = query.Title;
        findEvent.Description = query.Description;
        findEvent.StartAt = query.StartAt;
        findEvent.EndAt = query.EndAt;
        findEvent.TotalSeats = findEvent.TotalSeats;
        findEvent.AvailableSeats = findEvent.AvailableSeats;

        await _eventRepository.SaveChanges(ct);

        return new EventDto()
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

    public async Task DeleteEvent(Guid id, CancellationToken ct = default)
    {
        var existingEvent = await _eventRepository.GetById(id, ct);

        if (existingEvent is null)
            throw new NotFoundException($"Событие с id = {id} не удалось удалить");

        _eventRepository.Delete(existingEvent, ct);
        await _eventRepository.SaveChanges(ct);
    }
}