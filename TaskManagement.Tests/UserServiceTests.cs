using Moq;
using Microsoft.Extensions.Logging;
using TaskManagement.DAL.Repositories;
using TaskManagement.Domain.Models;
using TaskManagement.BL.Services.Implementation;
using TaskManagement.BL.Services;

namespace TaskManagement.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<ILogger<UserService>> _loggerMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _tokenServiceMock = new Mock<ITokenService>();
            _loggerMock = new Mock<ILogger<UserService>>();

            _userService = new UserService(_userRepositoryMock.Object, _loggerMock.Object, _tokenServiceMock.Object);
        }

        #region Register
        [Fact]
        public async System.Threading.Tasks.Task RegisterUserAsync_ReturnsNull_WhenEmailAlreadyExists()
        {
            // Arrange
            var email = "test@example.com";
            var username = "username";
            var password = "Password123!";
            var existingUser = new User { Email = email };

            _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(email))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _userService.RegisterUserAsync(username, email, password);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task RegisterUserAsync_ReturnsNull_WhenPasswordIsInvalid()
        {
            // Arrange
            var email = "test@example.com";
            var username = "username";
            var invalidPassword = "short";

            _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(email))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userService.RegisterUserAsync(username, email, invalidPassword);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task RegisterUserAsync_CreatesUserSuccessfully()
        {
            // Arrange
            var email = "test@example.com";
            var username = "username";
            var password = "Password123!";
            var user = new User
            {
                UserName = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(email))
                .ReturnsAsync((User)null);
            _userRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<User>())).Returns(System.Threading.Tasks.Task.CompletedTask);
            _userRepositoryMock.Setup(repo => repo.SaveChangesAsync()).Returns(System.Threading.Tasks.Task.CompletedTask);

            // Act
            var result = await _userService.RegisterUserAsync(username, email, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.UserName);
            Assert.Equal(email, result.Email);
            _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
            _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        #endregion
        #region Login
        [Fact]
        public async System.Threading.Tasks.Task AuthenticateUserAsync_ReturnsNull_WhenUserNotFound()
        {
            // Arrange
            var usernameOrEmail = "test@example.com";
            var password = "Password123!";

            _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(usernameOrEmail))
                .ReturnsAsync((User)null);
            _userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(usernameOrEmail))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userService.AuthenticateUserAsync(usernameOrEmail, password);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task AuthenticateUserAsync_ReturnsNull_WhenPasswordIsIncorrect()
        {
            // Arrange
            var usernameOrEmail = "test@example.com";
            var password = "WrongPassword";
            var user = new User
            {
                Email = usernameOrEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword")
            };

            _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(usernameOrEmail))
                .ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(usernameOrEmail))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userService.AuthenticateUserAsync(usernameOrEmail, password);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task AuthenticateUserAsync_ReturnsToken_WhenCredentialsAreCorrect()
        {
            // Arrange
            var usernameOrEmail = "test@example.com";
            var password = "Password123!";
            var user = new User
            {
                Email = usernameOrEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };
            var token = "fake-jwt-token";

            _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(usernameOrEmail))
                .ReturnsAsync(user);
            _tokenServiceMock.Setup(service => service.GenerateJwtToken(user))
                .Returns(token);

            // Act
            var result = await _userService.AuthenticateUserAsync(usernameOrEmail, password);

            // Assert
            Assert.Equal(token, result);
        }

        #endregion
    }
}
