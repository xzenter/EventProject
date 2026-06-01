using System.ComponentModel.DataAnnotations;

namespace EventProject.Presentation.Dto.Query;

/// <summary>
/// Параметры для создания события.
/// </summary>
public class EventForCreationQuery : IValidatableObject
{
    /// <summary>
    /// Название события.
    /// </summary>
    [Required]
    public required string Title { get; set; }

    /// <summary>
    /// Описание события.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Дата начала события.
    /// </summary>
    [Required]
    public required DateTime StartAt { get; set; }

    /// <summary>
    /// Дата окончания события.
    /// </summary>
    [Required]
    public required DateTime EndAt { get; set; }

    /// <summary>
    /// Общее количество мест на событие.
    /// </summary>
    public int TotalSeats { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartAt > EndAt)
        {
            yield return new ValidationResult("StartAt должна быть меньше EndAt");
        }

        if (TotalSeats <= 0)
            yield return new ValidationResult(
                "Общее количество мест должно быть положительным числом");
    }
}