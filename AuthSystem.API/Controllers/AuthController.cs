using AuthSystem.Application.Interfaces;
using AuthSystem.Domain.Exceptions;
using AuthSystem.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthSystem.Application.DTOs.Requests;
namespace AuthSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]

    public class AuthController : ControllerBase
    {
        private readonly IAuthApplicationService _authApplicationService;
        private readonly ITokenService _tokenService;

        public AuthController(IAuthApplicationService authApplicationService, ITokenService tokenService)
        {
            _authApplicationService = authApplicationService;
            _tokenService = tokenService;
        }



        [HttpGet("getUsers")]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var users = await _authApplicationService.GetUsersAsync(page, pageSize);
                return Ok(users);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving users" });
            }
        }
        [HttpGet("getUser")]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            try
            {
                var user = await _authApplicationService.GetUserByIdAsync(userId);
                return Ok(user);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving users" });
            }
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var userId = await _tokenService.GetUserIdFromTokenAsync(token);

                if (userId == null)
                    return Unauthorized();

                var user = await _authApplicationService.GetUserByIdAsync(userId.Value);
                if (user == null)
                    return Unauthorized();

                return Ok(user);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user information" });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _authApplicationService.LoginAsync(request);
                return Ok(result);
            }
            catch (UserNotFoundException)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }
            catch (InvalidCredentialsException)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }
            catch (InactiveUserException)
            {
                return Unauthorized(new { message = "Account is inactive" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }
      
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var result = await _authApplicationService.RegisterAsync(request);
                return Ok(result);
            }
            catch (UserAlreadyExistsException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred during registration" });
            }
        }
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest request)
        {
            try
            {
                var isValid = await _authApplicationService.ValidateTokenAsync(request.Token);
                return Ok(new { isValid });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while validating token" });
            }
        }
    }

    public class ValidateTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}
