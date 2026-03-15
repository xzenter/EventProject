using System.ComponentModel.DataAnnotations;

namespace EventProject.Controllers.Events.Dto;

/// <summary>
/// Параметры для обновления события.
/// </summary>
public class EventForUpdateDto : IValidatableObject
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
    public required DateTime StartAt { get; set; } // Возможно ?

    /// <summary>
    /// Дата окончания события.
    /// </summary>
    [Required]
    public required DateTime EndAt { get; set; } // Возможно ?

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartAt > EndAt)
        {
            yield return new ValidationResult("StartAt должна быть меньше EndAt");
        }
    }
}