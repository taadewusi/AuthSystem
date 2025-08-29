using AuthSystem.Domain.Entities;
using AuthSystem.Domain.Interfaces.Services;

namespace AuthSystem.Domain.Services
{
    public class UserDomainService
    {
        private readonly IPasswordService _passwordService;

        public UserDomainService(IPasswordService passwordService)
        {
            _passwordService = passwordService;
        }

        public User CreateUser(string email, string username, string password, string? firstName = null, string? lastName = null)
        {
            var passwordHash = _passwordService.HashPassword(password);
            return new User(email, username, passwordHash, firstName, lastName);
        }

        public bool ValidatePassword(User user, string password)
        {
            return _passwordService.VerifyPassword(password, user.PasswordHash);
        }
    }
}
