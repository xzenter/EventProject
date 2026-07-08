namespace EventProject.Shared;

public class BookingConfirmed
{
    /// <summary>
    /// Идентификатор брони
    /// </summary>
    public Guid BookingId { get; init; }

    /// <summary>
    /// Идентификатор события
    /// </summary>
    public Guid EventId { get; init; }

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Количество мест
    /// </summary>
    public int Seats { get; init; }

    /// <summary>
    /// Время подтверждения брони
    /// </summary>
    public DateTime ConfirmedAt { get; init; }
}