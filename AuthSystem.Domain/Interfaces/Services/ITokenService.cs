using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
        Task<bool> ValidateTokenAsync(string token);
        Task<Guid?> GetUserIdFromTokenAsync(string token);
    }
}
