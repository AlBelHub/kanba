using backend.Models;
using Task = System.Threading.Tasks.Task;
using KanbaTask = backend.Models.Task;

namespace backend.Repositories.Interfaces;

public interface ITaskRepository
{
    Task<KanbaTask> CreateTask(TaskProps taskProps);
    Task<KanbaTask?> GetTaskById(int id);
    Task<IEnumerable<KanbaTask>> GetTasksByBoardId(int boardId);
    Task<IEnumerable<KanbaTask>> GetTasksByColumnId(int columnId);
    Task<IEnumerable<KanbaTask>> GetTasksByUserId(int userId);
    Task<bool> UpdateTask(int id, string title, string? description, string status, int position, int? assignedTo);
    Task<bool> DeleteTask(int id);
}
