using Microsoft.Extensions.Logging;
using TaskManagement.DAL.Repositories;
using TaskManagement.Domain.Models;

namespace TaskManagement.BL.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly ITokenService _tokenService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _logger = logger;
            _tokenService = tokenService;
        }

        public async Task<User?> RegisterUserAsync(string username, string email, string password)
        {
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null)
            {
                _logger.LogWarning("Attempt to register with existing email: {Email}", email);
                return null;
            }

            if (!PasswordIsValid(password))
            {
                _logger.LogWarning("Registration failed: Password does not meet complexity requirements for user {Username}", username);
                return null;
            }

            var user = new User
            {
                UserName = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation("User {Username} registered successfully", username);
            return user;
        }

        public async Task<string?> AuthenticateUserAsync(string usernameOrEmail, string password)
        {
            var user = await _userRepository.GetByEmailAsync(usernameOrEmail) ??
                       await _userRepository.GetByUsernameAsync(usernameOrEmail);

            if (user == null)
            {
                _logger.LogWarning("User {UsernameOrEmail} not found.", usernameOrEmail);
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("Password of user {UsernameOrEmail} is not correct.", usernameOrEmail);
                return null;
            }

            string token = String.Empty;
            try
            {
                token = _tokenService.GenerateJwtToken(user);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            catch (Exception)
            {
                _logger.LogWarning("JWT token has not been created.");
                return null;
            }

            _logger.LogInformation("User {UsernameOrEmail} logged in successfully", usernameOrEmail);

            return token;
        }

        private static bool PasswordIsValid(string password)
        {
            if (password.Length < 8)
                return false;

            if (!password.Any(char.IsUpper))
                return false;

            if (!password.Any(char.IsLower))
                return false;

            if (!password.Any(char.IsDigit))
                return false;

            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
                return false;

            return true;
        }
    }
}
