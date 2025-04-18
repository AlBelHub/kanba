using System.Data;
using backend.Helpers;
using backend.Models;
using backend.Repositories.Abstract;
using backend.Repositories.Interfaces;
using Dapper;

namespace backend.Repositories;

public class UserRepository : RepositoryBase, IUserRepository
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUUIDProvider _uuidProvider;

    public UserRepository(IDbConnection db, IPasswordHasher pwdHash, IUUIDProvider uuidProvider) : base(db)
    {
        _passwordHasher = pwdHash;
        _uuidProvider = uuidProvider;
    }

    public async Task<string> GetPasswordHashAsync(string username)
    {
        string sql = "SELECT password FROM users WHERE username = @username";
        return await QueryFirstOrDefaultAsync<string>(sql, new {username = username});
    }

    public async Task<bool> VerifyPasswordHashAsync(string username, string password)
    {
        var sql = "SELECT password FROM users WHERE username = @username";
        var passwordHash = await QueryFirstOrDefaultAsync<string>(sql, new {username = username});

        Console.WriteLine(password);
        
        return _passwordHasher.VerifyHashedPassword(await GetPasswordHashAsync(password), passwordHash);
    }
    
    public async Task<bool> UserExistsAsync(string username)
    {
        string sql = "SELECT COUNT(1) FROM users WHERE username = @username";
        return await ExecuteScalarAsync<int>(sql, new {username = username}) > 0;
    }
    
    public async Task<User> RegisterUserAsync(string username, string password)
    {
        if (await UserExistsAsync(username))
        {
            return null;
        }
        
        var passwordHash = _passwordHasher.HashPassword(password);
        var id = _uuidProvider.GenerateUUIDv7();
        
        string sql = @"INSERT INTO users (id, username, password) 
                        VALUES (@id, @username, @password)
                        RETURNING *";
        return await _db.QueryFirstOrDefaultAsync<User>(sql, new {id = id,username = username, password = passwordHash});
    }
    
    public async Task<Guid> GetUserId(string username)
    {
        string sql = "SELECT id FROM users WHERE username = @username";
        return await _db.QueryFirstOrDefaultAsync<Guid>(sql, new {username = username});
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        string sql = "SELECT id, username FROM users";
        return await QueryAsync<User>(sql);
    }
}