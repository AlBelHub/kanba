using backend.Models;

namespace backend.Repositories.Interfaces;

public interface IUserRepository
{
    Task<string> GetPasswordHashAsync(string username);
    Task<bool> VerifyPasswordHashAsync(string username, string passwordHash);
    Task<bool> UserExistsAsync(string username);
    Task<bool> RegisterUserAsync(string username, string password);
    Task<IEnumerable<User>> GetUsersAsync();
}