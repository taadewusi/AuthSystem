using AuthSystem.Application.DTOs.Requests;
using AuthSystem.Application.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthSystem.Application.Interfaces
{
    public interface IAuthApplicationService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<IEnumerable<UserDto?>> GetUsersAsync(int page, int pageSize);
        Task<bool> ValidateTokenAsync(string token);
    }
}
