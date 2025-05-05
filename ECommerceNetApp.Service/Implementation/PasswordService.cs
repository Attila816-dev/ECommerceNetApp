using ECommerceNetApp.Domain.Interfaces;
using BC = BCrypt.Net.BCrypt;

namespace ECommerceNetApp.Service.Implementation
{
    public class PasswordService : IPasswordService
    {
        public string HashPassword(string password)
        {
            return BC.HashPassword(password);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            return BC.Verify(password, passwordHash);
        }
    }
}
