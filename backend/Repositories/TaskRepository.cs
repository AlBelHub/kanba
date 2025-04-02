using System.Data;
using backend.Helpers;
using backend.Models;
using backend.Repositories.Abstract;
using backend.Repositories.Interfaces;
using Dapper;
using Task = System.Threading.Tasks.Task;

namespace backend.Repositories;

public class TaskRepository : RepositoryBase, ITaskRepository
{
    private readonly IUUIDProvider _uuidProvider;
    public TaskRepository(IDbConnection db, IUUIDProvider uuidProvider) : base(db)
    {
        _uuidProvider = uuidProvider;
    }

    public async Task<Task> CreateTask(TaskProps taskProps)
    {
        string sql = @"
        INSERT INTO Tasks (
            id,
            column_id, 
            board_id, 
            title, 
            description, 
            status, 
            position,
            created_by, 
            assigned_to
        ) 
        VALUES (
            @id,
            @ColumnId,
            @BoardId,
            @Title, 
            @Description, 
            @Status, 
            @Position,
            @CreatedBy, 
            @AssignedTo
        )
        RETURNING *";

        return await _db.QueryFirstOrDefaultAsync<Task>(sql, new
        {
            id = _uuidProvider.GenerateUUIDv7(),
            ColumnId = taskProps.column_id,
            BoardId = taskProps.board_id,
            Title = taskProps.title,
            Description = taskProps.description,
            Status = taskProps.status,
            Position = taskProps.position,
            CreatedBy = taskProps.created_by,
            AssignedTo = taskProps.created_by // Присваивается создателю при создании
        });
    }

    public Task<Task?> GetTaskById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Task>> GetTasksByBoardId(int boardId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Task>> GetTasksByColumnId(int columnId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Task>> GetTasksByUserId(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateTask(int id, string title, string? description, string status, int position, int? assignedTo)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteTask(int id)
    {
        throw new NotImplementedException();
    }
}