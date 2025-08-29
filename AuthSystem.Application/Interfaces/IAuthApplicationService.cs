using AuthSystem.Application.DTOs.Requests;
using AuthSystem.Application.DTOs.Responses;

namespace AuthSystem.Application.Interfaces
{
    public interface IAuthApplicationService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<UserDto?> GetUserByIdAsync(Guid userId);
        Task<IEnumerable<UserDto?>> GetUsersAsync(int page, int pageSize);
        Task<bool> ValidateTokenAsync(string token);
    }
}
