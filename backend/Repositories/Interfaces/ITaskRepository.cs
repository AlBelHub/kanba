using backend.Models;
using Task = System.Threading.Tasks.Task;
using KanbaTask = backend.Models.Task;

namespace backend.Repositories.Interfaces;

public interface ITaskRepository
{
    Task<KanbaTask> CreateTask(TaskProps taskProps);
    Task<KanbaTask?> GetTaskById(Guid id);
    Task<IEnumerable<KanbaTask>> GetTasksByBoardId(Guid boardId);
    Task<TaskWithUsersDto> getTaskDetails(Guid id);
    Task<IEnumerable<KanbaTask>> GetTasksByColumnId(Guid columnId);
    Task<IEnumerable<KanbaTask>> GetTasksByUserId(Guid userId);
    Task<bool> UpdateTask(Guid id, TaskProps props);
    Task<bool> DeleteTask(Guid id);
}
