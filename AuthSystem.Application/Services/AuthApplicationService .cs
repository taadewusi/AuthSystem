using AuthSystem.Application.DTOs.Requests;
using AuthSystem.Application.DTOs.Responses;
using AuthSystem.Application.Interfaces;
using AuthSystem.Domain.Entities;
using AuthSystem.Domain.Exceptions;
using AuthSystem.Domain.Interfaces.Repositories;
using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthSystem.Application.Services
{
    public class AuthApplicationService : IAuthApplicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly UserDomainService _userDomainService;

        public AuthApplicationService(
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            UserDomainService userDomainService)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _userDomainService = userDomainService;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Email and password are required");

            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

            if (user == null)
                throw new UserNotFoundException(request.Email);

            if (!user.IsActive)
                throw new InactiveUserException();

            if (!_userDomainService.ValidatePassword(user, request.Password))
                throw new InvalidCredentialsException();

            // Update last login
            user.UpdateLastLogin();
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Generate token
            var token = _tokenService.GenerateToken(user);
            var expiresAt = DateTime.UtcNow.AddHours(24);

            return new AuthResponse
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = MapToUserDto(user)
            };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            // Validate input
            ValidateRegistrationRequest(request);

            // Check if user already exists
            if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
                throw new UserAlreadyExistsException("email", request.Email);

            if (await _unitOfWork.Users.UsernameExistsAsync(request.Username))
                throw new UserAlreadyExistsException("username", request.Username);

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Create user using domain service
                var user = _userDomainService.CreateUser(
                    request.Email,
                    request.Username,
                    request.Password,
                    request.FirstName,
                    request.LastName);

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                // Generate token
                var token = _tokenService.GenerateToken(user);
                var expiresAt = DateTime.UtcNow.AddHours(24);

                await _unitOfWork.CommitTransactionAsync();

                return new AuthResponse
                {
                    Token = token,
                    ExpiresAt = expiresAt,
                    User = MapToUserDto(user)
                };
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            return user != null ? MapToUserDto(user) : null;
        }
        public async Task<IEnumerable<UserDto?>> GetUsersAsync(int page, int pageSize)
        {
            var users = await _unitOfWork.Users.GetAllAsync();

            if (users == null || !users.Any())
                return new List<UserDto>();
            var result = MapToUserDtos(users);
            result.OrderBy(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
            return result;
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            return await _tokenService.ValidateTokenAsync(token);
        }

        private static void ValidateRegistrationRequest(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required");

            if (string.IsNullOrWhiteSpace(request.Username))
                throw new ArgumentException("Username is required");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Password is required");

            if (request.Password.Length < 6)
                throw new ArgumentException("Password must be at least 6 characters long");

            if (request.Username.Length < 3)
                throw new ArgumentException("Username must be at least 3 characters long");

            // Validate email format
            try
            {
                _ = new Email(request.Email);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException("Invalid email format");
            }
        }

        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = user.Roles.ToList(),
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }

        // Add this new method to handle list mapping
        private static IEnumerable<UserDto> MapToUserDtos(IEnumerable<User> users)
        {
            return users.Select(MapToUserDto).ToList();
        }
    }
}
