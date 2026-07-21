using EventProject.Events.Application.Abstractions.Repositories;
using EventProject.Events.Application.Abstractions.Services;
using EventProject.Events.Application.Caching;
using EventProject.Events.Application.DTOs;
using EventProject.Events.Application.Events.DTOs;
using EventProject.Events.Domain.Exceptions;
using Microsoft.Extensions.Options;

namespace EventProject.Events.Application.Events;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly ICacheService _cacheService;
    private readonly CacheTtlOptions _options;

    public EventService(IEventRepository eventRepository, ICacheService cacheService, IOptions<CacheTtlOptions> options)
    {
        _eventRepository = eventRepository;
        _cacheService = cacheService;
        _options = options.Value;
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
        // Сначала проверяем Redis
        var cachedEvent = await _cacheService
            .GetAsync<EventDto>(CacheKeys.Event(id), ct);

        if (cachedEvent is not null)
            return cachedEvent;

        // Если в Redis нет данных - читаем из БД
        var findEvent = await _eventRepository.GetById(id, ct);

        if (findEvent == null)
        {
            throw new NotFoundException($"Событие с id = {id} не найдено");
        }

        var eventDto = new EventDto
        {
            Id = findEvent.Id,
            Title = findEvent.Title,
            Description = findEvent.Description,
            StartAt = findEvent.StartAt,
            EndAt = findEvent.EndAt,
            TotalSeats = findEvent.TotalSeats,
            AvailableSeats = findEvent.AvailableSeats
        };

        // Сохраняем результат в Redis с TTL
        await _cacheService
            .SetAsync(
                CacheKeys.Event(id),
                eventDto,
                TimeSpan.FromMinutes(_options.EventMinutes),
                ct
            );

        return eventDto;
    }

    public async Task<IReadOnlyCollection<EventDto>> GetTopEvents(CancellationToken ct = default)
    {
        // Сначала проверяем Redis
        var cachedTopEvents = await _cacheService
            .GetAsync<IReadOnlyCollection<EventDto>>(CacheKeys.Top10Events(), ct);

        if (cachedTopEvents is not null)
            return cachedTopEvents;

        // Если в Redis нет данных - читаем из БД
        var events = await _eventRepository.GetTopEvents(10, ct);

        var response = events
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

        // Сохраняем результат в Redis с TTL
        await _cacheService
            .SetAsync(
                CacheKeys.Top10Events(),
                response,
                TimeSpan.FromMinutes(_options.Top10EventsMinutes),
                ct
            );

        return response;
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
            throw new BadRequestException("Дата начала события не может быть позже даты окончания");

        var findEvent = await _eventRepository.GetById(id, ct);

        if (findEvent == null)
            throw new NotFoundException($"Событие с id = {id} не найдено");

        findEvent.Title = query.Title;
        findEvent.Description = query.Description;
        findEvent.StartAt = query.StartAt;
        findEvent.EndAt = query.EndAt;
        findEvent.TotalSeats = findEvent.TotalSeats;
        findEvent.AvailableSeats = findEvent.AvailableSeats;

        await _eventRepository.SaveChanges(ct);

        var eventDto = new EventDto()
        {
            Id = findEvent.Id,
            Title = findEvent.Title,
            Description = findEvent.Description,
            StartAt = findEvent.StartAt,
            EndAt = findEvent.EndAt,
            TotalSeats = findEvent.TotalSeats,
            AvailableSeats = findEvent.AvailableSeats
        };

        // Обновляем запись в кэша сразу после изменения, стратегия Update-on-Write и устанавливаем TTL
        await _cacheService
            .SetAsync(
                CacheKeys.Event(id),
                eventDto,
                TimeSpan.FromMinutes(_options.EventMinutes),
                ct
            );

        return eventDto;
    }

    public async Task DeleteEvent(Guid id, CancellationToken ct = default)
    {
        var existingEvent = await _eventRepository.GetById(id, ct);

        if (existingEvent is null)
            throw new NotFoundException($"Событие с id = {id} не удалось удалить");

        _eventRepository.Delete(existingEvent, ct);
        await _eventRepository.SaveChanges(ct);

        // Удаляем запись из кэша
        await _cacheService.RemoveAsync(CacheKeys.Event(id), ct);
    }
}