namespace EventProject.Presentation.Dto;

/// <summary>
/// Объект, содержащий результаты пагинации.
/// </summary>
public class PaginatedResult<T>
{
    /// <summary>
    /// Список объектов.
    /// </summary>
    public required IEnumerable<T> Items { get; init; }

    /// <summary>
    /// Номер текущей страницы.
    /// </summary>
    public required int Page { get; init; }

    /// <summary>
    /// Количество объектов на странице.
    /// </summary>
    public required int PageSize { get; init; }

    /// <summary>
    /// Общее количество объектов.
    /// </summary>
    public required int TotalItems { get; init; }

    /// <summary>
    /// Общее количество страниц.
    /// </summary>
    public required int TotalPages { get; init; }
}