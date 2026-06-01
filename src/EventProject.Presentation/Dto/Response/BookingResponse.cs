using EventProject.Domain.Entities;

namespace EventProject.Presentation.Dto.Response;

/// <summary>
/// Объект бронирования.
/// </summary>
public class BookingDto
{
    /// <summary>
    /// Идентификатор бронирования.
    /// </summary>
    public Guid BookingId { get; set; }

    /// <summary>
    /// Идентификатор события.
    /// </summary>
    public Guid EventId { get; set; }

    /// <summary>
    /// Статус бронирования.
    /// </summary>
    public BookingStatus Status { get; set; }
}