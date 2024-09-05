using TaskManagement.Domain.Models;

namespace TaskManagement.BL.Services
{
    public interface ITokenService
    {
        string GenerateJwtToken(User user);
    }
}
