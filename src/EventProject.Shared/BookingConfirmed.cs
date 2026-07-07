namespace EventProject.Shared;

public class BookingConfirmed
{
    /*
     *  простой неизменяемый тип с минимально необходимыми данными:
     * идентификаторы брони,
     * события и
     * пользователя,
     * количество мест
     * момент подтверждения.
     */

    public Guid BookingId { get; init; }
    public Guid EventId { get; init; }
    public Guid UserId { get; init; }
    public DateTime CreatedAt { get; init; }
}