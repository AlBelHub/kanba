using backend.Models;

namespace backend.Repositories.Interfaces;

public interface ISpaceRepository
{
    Task<Space> CreateSpace(Guid userId, string spaceName);
    Task<Space?> GetSpace(string spaceId);
    Task<IEnumerable<Space>> GetSpacesByUser(string userId);
    Task<bool> ChangeSpace(string spaceId, Space space);
    Task<int> DeleteSpace(string userId, string spaceId);
}