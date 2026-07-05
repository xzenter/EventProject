using System.ComponentModel.DataAnnotations;

namespace EventProject.Events.Application.Events.DTOs;

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
    [Range(1, int.MaxValue,
        ErrorMessage = "Номер текущей страницы не может быть меньше 1 и больше 2147483647")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Количество элементов на странице.
    /// </summary>
    [Range(1, int.MaxValue,
        ErrorMessage = "Количество элементов на странице не может быть меньше 1 и больше 2147483647")]
    public int PageSize { get; set; } = 10;
}