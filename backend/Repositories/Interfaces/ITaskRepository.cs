using backend.Models;
using Task = System.Threading.Tasks.Task;

namespace backend.Repositories.Interfaces;

public interface ITaskRepository
{
    Task<Task> CreateTask(TaskProps taskProps);
    Task<Task?> GetTaskById(int id);
    Task<IEnumerable<Task>> GetTasksByBoardId(int boardId);
    Task<IEnumerable<Task>> GetTasksByColumnId(int columnId);
    Task<IEnumerable<Task>> GetTasksByUserId(int userId);
    Task<bool> UpdateTask(int id, string title, string? description, string status, int position, int? assignedTo);
    Task<bool> DeleteTask(int id);
}
