namespace EventProject.Dto.Response;

/// <summary>
/// Объект, описывающий событие.
/// </summary>
public class EventDto
{
    /// <summary>
    /// Идентификатор события.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Название события.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Описание события.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Дата начала события.
    /// </summary>
    public required DateTime StartAt { get; init; }

    /// <summary>
    /// Дата окончания события.
    /// </summary>
    public required DateTime EndAt { get; init; }
}