using backend.Models;

namespace backend.Repositories.Interfaces;

public interface IUserRepository
{
    Task<string> GetPasswordHashAsync(string username);
    Task<bool> VerifyPasswordHashAsync(string username, string password);
    Task<bool> UserExistsAsync(string username);
    Task<User> RegisterUserAsync(string username, string password);
    Task<Guid> GetUserId(string username);
    Task<IEnumerable<User>> GetUsersAsync();
}