using EventProject.Bookings.Domain.Enums;

namespace EventProject.Bookings.Application.Booking.DTOs;

public class BookingInfo
{
    /// <summary>
    /// Идентификатор брони
    /// </summary>
    public required Guid BookingId { get; init; }

    /// <summary>
    /// Идентификатор события, к которому относится бронь
    /// </summary>
    public required Guid EventId { get; init; }

    /// <summary>
    /// Идентификатор пользователя, который бронирует событие
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Статус брони
    /// </summary>
    public required BookingStatus Status { get; init; }

    /// <summary>
    /// Дата и время создания брони
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Дата и время обработки
    /// </summary>
    public required DateTime? ProcessedAt { get; init; }
}