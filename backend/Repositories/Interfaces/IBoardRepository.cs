using backend.Models;

namespace backend.Repositories.Interfaces;

public interface IBoardRepository
{
    Task<Board> CreateBoard(BoardsProps boardsProps);
    Task<Board?> GetBoardById(int id);
    Task<IEnumerable<Board>> GetBoardsByUserId(int userId);  
    Task<IEnumerable<Board>> GetBoardsBySpaceId(int spaceId);
    Task<bool> UpdateBoard(int id, BoardsProps boardsProps);
    Task<bool> DeleteBoard(int id);
}