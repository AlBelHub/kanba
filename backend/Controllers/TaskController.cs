using System.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskController : KanbaControllerBase
{
    private readonly ITaskRepository _taskRepository;
    
    public TaskController(IDbConnection db, ITaskRepository taskRepository) : base(db)
    {
        _taskRepository = taskRepository;        
    }
    
     [HttpGet("getTasks")]
    public async Task<IActionResult> GetTasks()
    {
        var tasks = await _db.QueryAsync(
            "SELECT id, column_id, title, description, status, created_by, assigned_to FROM Tasks");

        return Ok(tasks);
    }

    [HttpPost("createTask")]
    public async Task<IActionResult> CreateTask([FromBody] TaskProps props)
    {
       
        var newTask = await _taskRepository.CreateTask(props);

        if (newTask != null)
        {
            return Ok(newTask);
        }

        return BadRequest("Error creating task");
    }

    [HttpPost("moveTask")]
    public async Task<IActionResult> MoveTask([FromBody] TaskMoveRequest request)
    {
        if (_db.State != ConnectionState.Open)
        {
            _db.Open();
        }

        using var transaction = _db.BeginTransaction();
        try
        {
            if (request.OldColumnId == request.NewColumnId)
            {
                if (request.TaskOldPos < request.TaskNewPos)
                {
                    await _db.ExecuteAsync(
                        "UPDATE tasks SET position = position - 1 WHERE column_id = @ColumnId AND position > @TaskOldPos AND position <= @TaskNewPos",
                        new { ColumnId = request.OldColumnId, request.TaskOldPos, request.TaskNewPos }, transaction);
                }
                else
                {
                    await _db.ExecuteAsync(
                        "UPDATE tasks SET position = position + 1 WHERE column_id = @ColumnId AND position >= @TaskNewPos AND position < @TaskOldPos",
                        new { ColumnId = request.OldColumnId, request.TaskOldPos, request.TaskNewPos }, transaction);
                }

                await _db.ExecuteAsync(
                    "UPDATE tasks SET position = @TaskNewPos WHERE id = @TaskId",
                    new { request.TaskNewPos, request.TaskId }, transaction);
            }
            else
            {
                await _db.ExecuteAsync(
                    "UPDATE tasks SET position = position - 1 WHERE column_id = @OldColumnId AND position > @TaskOldPos",
                    new { request.OldColumnId, request.TaskOldPos }, transaction);

                await _db.ExecuteAsync(
                    "UPDATE tasks SET column_id = @NewColumnId, position = @TaskNewPos WHERE id = @TaskId",
                    new { request.NewColumnId, request.TaskNewPos, request.TaskId }, transaction);

                await _db.ExecuteAsync(
                    "UPDATE tasks SET position = position + 1 WHERE column_id = @NewColumnId AND position >= @TaskNewPos AND id <> @TaskId",
                    new { request.NewColumnId, request.TaskNewPos, request.TaskId }, transaction);
            }

            transaction.Commit();
            return Ok(new { message = "Task moved successfully" });
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return Problem(ex.Message);
        }
    }
}