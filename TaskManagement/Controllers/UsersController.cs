using Microsoft.AspNetCore.Mvc;
using TaskManagement.BL.DTO.User;
using TaskManagement.BL.Services;

namespace TaskManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid registration data.");
            }

            var user = await _userService.RegisterUserAsync(registerDto.Username, registerDto.Email, registerDto.Password);
            if (user == null)
            {
                _logger.LogWarning("Failed to register user with email {Email}.", registerDto.Email);
                return BadRequest("User already exists or invalid password.");
            }

            _logger.LogInformation("User {Username} registered successfully.", registerDto.Username);
            return Ok("User registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid login data.");
            }

            var token = await _userService.AuthenticateUserAsync(loginDto.UsernameOrEmail, loginDto.Password);
            if (token == null)
            {
                _logger.LogWarning("Failed login attempt for {UsernameOrEmail}.", loginDto.UsernameOrEmail);
                return Unauthorized("Invalid credentials.");
            }

            _logger.LogInformation("User {UsernameOrEmail} logged in successfully.", loginDto.UsernameOrEmail);
            return Ok(new { Token = token });
        }
    }
}
