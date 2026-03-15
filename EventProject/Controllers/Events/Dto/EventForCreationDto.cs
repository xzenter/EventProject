using System.ComponentModel.DataAnnotations;

namespace EventProject.Controllers.Events.Dto;

public class EventForCreationDto : IValidatableObject
{
    [Required]
    public required string Title { get; set; }

    public string? Description { get; set; }

    [Required]
    public required DateTime StartAt { get; set; } // Возможно ?

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