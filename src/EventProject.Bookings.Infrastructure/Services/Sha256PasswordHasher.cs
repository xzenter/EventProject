using System.Security.Cryptography;
using System.Text;
using EventProject.Bookings.Application.Abstractions.Services;

namespace EventProject.Bookings.Infrastructure.Services
{
    public sealed class Sha256PasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));

            return Convert.ToHexString(bytes);
        }

        public bool Verify(string password, string passwordHash)
        {
            var computedHash = Hash(password);

            return string.Equals(computedHash, passwordHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
