using System.Data;
using backend.Helpers;
using backend.Models;
using backend.Repositories.Abstract;
using backend.Repositories.Interfaces;

namespace backend.Repositories;

public class UserRepository : RepositoryBase, IUserRepository
{
    private readonly IPasswordHasher _passwordHasher;

    public UserRepository(IDbConnection db, IPasswordHasher pwdHash) : base(db)
    {
        _passwordHasher = pwdHash;
    }

    public async Task<string> GetPasswordHashAsync(string username)
    {
        string sql = "SELECT password FROM users WHERE username = @username";
        return await QueryFirstOrDefaultAsync<string>(sql, new {username = username});
    }

    public async Task<bool> VerifyPasswordHashAsync(string username, string passwordHash)
    {
        return _passwordHasher.VerifyHashedPassword(await GetPasswordHashAsync(username), passwordHash);
    }
    
    public async Task<bool> UserExistsAsync(string username)
    {
        string sql = "SELECT * FROM users WHERE username = @username";
        return await ExecuteScalarAsync<bool>(sql, new {username = username});
    }
    
    public async Task<bool> RegisterUserAsync(string username, string password)
    {
        if (await UserExistsAsync(username))
        {
            return false;
        }
        
        var passwordHash = _passwordHasher.HashPassword(password);

        string sql = "INSERT INTO users (username, password) VALUES (@username, @passwordHash)";
        return await ExecuteScalarAsync<bool>(sql, new {username = username, passwordHash = passwordHash});
    }
    
    

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        string sql = "SELECT id, username FROM users";
        return await QueryAsync<User>(sql);
    }
}