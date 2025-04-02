using System.Data;
using backend.Helpers;
using backend.Models;
using backend.Repositories.Abstract;
using backend.Repositories.Interfaces;
using Dapper;

namespace backend.Repositories;

public class SpaceRepository : RepositoryBase, ISpaceRepository
{
    private readonly IUUIDProvider _uuidProvider;
    public SpaceRepository(IDbConnection db, IUUIDProvider uuidProvider) : base(db)
    {
        _uuidProvider = uuidProvider;
    }

    public async Task<Space> CreateSpace(Guid userId, string spaceName)
    {
        var id = _uuidProvider.GenerateUUIDv7();
        
        string sql = "INSERT INTO spaces (id, name, user_id) VALUES (@id, @spaceName, @userId) RETURNING *";
        return await _db.QueryFirstOrDefaultAsync<Space>(sql, new { id, userId, spaceName });
    }

    public Task<Space?> GetSpace(string spaceId)
    {
        string sql = "SELECT * FROM spaces WHERE id = @spaceId";
        return _db.QueryFirstOrDefaultAsync<Space>(sql, new {spaceId});
    }

    public async Task<IEnumerable<Space>> GetSpacesByUser(string userId)
    {
        string sql = "SELECT * FROM spaces WHERE user_id = @userId";
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