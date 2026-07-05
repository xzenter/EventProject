using EventProject.Events.Domain.Enums;

namespace EventProject.Events.Domain.Entities;

public class User
{
    public required Guid UserId { get; set; }

    public required string Login { get; set; }

    public required Role Role { get; set; }
}