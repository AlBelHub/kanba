using System.Data;
using backend.Models;
using backend.Repositories.Abstract;
using backend.Repositories.Interfaces;
using Dapper;

namespace backend.Repositories;

public class SpaceRepository : RepositoryBase, ISpaceRepository
{
    public SpaceRepository(IDbConnection db) : base(db) { }

    public async Task<Space> CreateSpace(string userId, string spaceName)
    {
        string sql = "INSERT INTO spaces (name, userId) VALUES (@spaceName, @userId) RETURNING *";
        return await _db.QueryFirstOrDefaultAsync<Space>(sql, new { userId, spaceName });
    }

    public Task<Space?> GetSpace(string spaceId)
    {
        string sql = "SELECT * FROM spaces WHERE id = @spaceId";
        return _db.QueryFirstOrDefaultAsync<Space>(sql, new {spaceId});
    }

    public async Task<IEnumerable<Space>> GetSpacesByUser(string userId)
    {
        string sql = "SELECT * FROM spaces WHERE userId = @userId";
        return await _db.QueryAsync<Space>(sql, new {userId});
    }

    public Task<bool> ChangeSpace(string spaceId, Space space)
    {
        throw new NotImplementedException();
    }

    public async Task<int> DeleteSpace(string userId, string spaceId)
    {
        string sql = "DELETE FROM spaces WHERE id = @spaceId";
        return await _db.ExecuteAsync(sql, new {spaceId});
    }
}