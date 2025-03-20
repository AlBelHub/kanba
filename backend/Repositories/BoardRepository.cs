using System.Data;
using backend.Models;
using backend.Repositories.Abstract;
using backend.Repositories.Interfaces;
using Dapper;

namespace backend.Repositories;

public class BoardRepository : RepositoryBase, IBoardRepository
{
    public BoardRepository(IDbConnection db) : base(db)
    {
    }

    public async Task<Board> CreateBoard(BoardsProps boardsProps)
    {

        string sql = @"
            INSERT INTO Boards (name, space_id, owner_id) 
            SELECT @Name, @SpaceId, @OwnerId
            WHERE NOT EXISTS (
                SELECT 1 FROM Boards 
                WHERE name = @Name AND space_id = @SpaceId
            )";
        
        return await _db.QueryFirstOrDefaultAsync<Board>(sql, boardsProps);
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