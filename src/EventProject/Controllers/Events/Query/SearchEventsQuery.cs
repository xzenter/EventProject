namespace EventProject.Controllers.Events.Query;

/// <summary>
/// Параметры для поиска событий.
/// </summary>
public class SearchEventsQuery
{
    /// <summary>
    /// Название события.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Дата начала события.
    /// </summary>
    public DateTime? From { get; set; }

    /// <summary>
    /// Дата окончания события.
    /// </summary>
    public DateTime? To { get; set; }

    /// <summary>
    /// Номер текущей страницы.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Количество элементов на странице.
    /// </summary>
    public int PageSize { get; set; } = 10;
}