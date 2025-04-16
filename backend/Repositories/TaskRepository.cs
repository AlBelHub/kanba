using System.Data;
using backend.Helpers;
using backend.Models;
using backend.Repositories.Abstract;
using backend.Repositories.Interfaces;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using KanbaTask = backend.Models.Task;


namespace backend.Repositories;

public class TaskRepository : RepositoryBase, ITaskRepository
{
    private readonly IUUIDProvider _uuidProvider;
    public TaskRepository(IDbConnection db, IUUIDProvider uuidProvider) : base(db)
    {
        _uuidProvider = uuidProvider;
    }

    public async Task<KanbaTask> CreateTask(TaskProps taskProps)
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

        return await _db.QueryFirstOrDefaultAsync<KanbaTask>(sql, new
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

    public Task<KanbaTask?> GetTaskById(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<KanbaTask>> GetTasksByBoardId(Guid boardId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<KanbaTask>> GetTasksByColumnId(Guid columnId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<KanbaTask>> GetTasksByUserId(Guid userId)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> UpdateTask(Guid id, TaskProps props)
    {
        string sql = @"
        UPDATE Tasks
        SET 
            title = @Title,
            description = @Description,
            status = @Status,
            position = @Position,
            assigned_to = @AssignedTo
        WHERE id = @Id";

        int affectedRows = await _db.ExecuteAsync(sql, new
        {
            Id = id,
            Title = props.title,
            Description = props.description,
            Status = props.status,
            Position = props.position,
            AssignedTo = props.assigned_to
        });

        return affectedRows > 0;
    }

    public async Task<TaskWithUsersDto> getTaskDetails(Guid id)
    {
        string sql = @"SELECT 
                        t.*, 
                        u1.id AS created_by_id, u1.username AS created_by_username,
                        u2.id AS assigned_to_id, u2.username AS assigned_to_username
                        FROM Tasks t
                        LEFT JOIN Users u1 ON t.created_by = u1.id
                        LEFT JOIN Users u2 ON t.assigned_to = u2.id
                        WHERE t.id = @Id";
        
        var task = await _db.QueryFirstOrDefaultAsync<TaskWithUsersDto>(sql, new { Id = id });
        
        return task;
    }
    
    public async Task<bool> DeleteTask(Guid id)
    {
        string sql = "DELETE FROM Tasks WHERE id = @Id";

        int affectedRows = await _db.ExecuteAsync(sql, new { Id = id });

        return affectedRows > 0;
    }
}