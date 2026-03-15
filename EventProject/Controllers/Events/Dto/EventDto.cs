namespace EventProject.Controllers.Events.Dto;

/// <summary>
/// Объект, описывающий событие.
/// </summary>
public class EventDto
{
    /// <summary>
    /// Идентификатор события.
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Название события.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Описание события.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Дата начала события.
    /// </summary>
    public required DateTime StartAt { get; set; }

    /// <summary>
    /// Дата окончания события.
    /// </summary>
    public required DateTime EndAt { get; set; }
}