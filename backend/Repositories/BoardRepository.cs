using System.Data;
using backend.Helpers;
using backend.Models;
using backend.Repositories.Abstract;
using backend.Repositories.Interfaces;
using Dapper;

namespace backend.Repositories;

public class BoardRepository : RepositoryBase, IBoardRepository
{
    private readonly IUUIDProvider _uuidProvider;
    
    public BoardRepository(IDbConnection db, IUUIDProvider uuidProvider) : base(db)
    {
        _uuidProvider = uuidProvider;
    }

    public async Task<Board> CreateBoard(BoardsProps boardsProps)
    {
        var id = _uuidProvider.GenerateUUIDv7();
    
        string sql = @"
        INSERT INTO Boards (id, name, space_id, owner_id) 
        SELECT @Id, @Name, @SpaceId, @OwnerId
        WHERE NOT EXISTS (
            SELECT 1 FROM Boards 
            WHERE name = @Name AND space_id = @SpaceId
        )
        RETURNING *";

        return await _db.QueryFirstOrDefaultAsync<Board>(sql, new 
        { 
            Id = id, 
            Name = boardsProps.name, 
            SpaceId = boardsProps.space_id, 
            OwnerId = boardsProps.owner_id
        });
    }

    public Task<Board?> GetBoardById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Board>> GetBoardsByUserId(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Board>> GetBoardsBySpaceId(int spaceId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateBoard(int id, BoardsProps boardsProps)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteBoard(int id)
    {
        throw new NotImplementedException();
    }
}