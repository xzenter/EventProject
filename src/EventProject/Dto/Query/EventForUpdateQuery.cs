using System.ComponentModel.DataAnnotations;

namespace EventProject.Dto.Query;

/// <summary>
/// Параметры для обновления события.
/// </summary>
public class EventForUpdateQuery : IValidatableObject
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

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartAt > EndAt)
        {
            yield return new ValidationResult("StartAt должна быть меньше EndAt");
        }
    }
}