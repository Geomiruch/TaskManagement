using TaskManagement.Domain.Models;

namespace TaskManagement.BL.Services
{
    public interface IUserService
    {
        Task<User?> RegisterUserAsync(string username, string email, string password);
        Task<string?> AuthenticateUserAsync(string usernameOrEmail, string password);
    }
}
