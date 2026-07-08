using EventProject.Users.Domain.Enums;

namespace EventProject.Users.Domain.Entities
{
    public class User
    {
        public User()
        {
        }
        
        public required Guid UserId { get; set; }

        public required string Login { get; set; }

        public required string PasswordHash { get; set; }

        public required Role Role { get; set; }
    }
}