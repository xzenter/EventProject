using EventProject.DataAccess;
using EventProject.Dto;
using EventProject.Dto.Query;
using EventProject.Dto.Response;
using EventProject.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace EventProject.Services.Event;

public class EventService : IEventService
{
    private readonly AppDbContext _appDbContext;

    public EventService(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<PaginatedResult<EventDto>> GetEvents(SearchEventsQuery query, CancellationToken ct = default)
    {
        var filtered = _appDbContext.Events
            .Where(e => (string.IsNullOrWhiteSpace(query.Title) || e.Title.Contains(query.Title)) &&
                        (!query.From.HasValue || e.StartAt >= query.From) &&
                        (!query.To.HasValue || e.EndAt <= query.To));

        var filteredCount = await filtered.CountAsync(ct);

        var entities = await filtered
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        var items = entities
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
        var findEvent = await _appDbContext.Events.FirstOrDefaultAsync(e => e.Id == id, ct);

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

        await _appDbContext.Events.AddAsync(newEvent, ct);
        await _appDbContext.SaveChangesAsync(ct);

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

        var findEvent = await _appDbContext.Events.FirstOrDefaultAsync(e => e.Id == id, ct);

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

        await _appDbContext.SaveChangesAsync(ct);

        return new EventDto()
        {
            Id = findEvent.Id,
            Title = findEvent.Title,
            Description = findEvent.Description,
            StartAt = findEvent.StartAt,
            EndAt = findEvent.EndAt
        };
    }

    public async Task DeleteEvent(Guid id, CancellationToken ct = default)
    {
        var existingEvent = await _appDbContext.Events.FirstOrDefaultAsync(e => e.Id == id, ct);

        if (existingEvent is null)
            throw new NotFoundException($"Событие с id = {id} не удалось удалить");

        _appDbContext.Events.Remove(existingEvent);

        await _appDbContext.SaveChangesAsync(ct);
    }
}