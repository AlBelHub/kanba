using System.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ColumnsController : KanbaControllerBase
{
    private readonly IColumnRepository _columnRepository;

    public ColumnsController(IDbConnection db, IColumnRepository columnRepository) : base(db)
    {
        _columnRepository = columnRepository;
    }
    
    [HttpGet("getByBoardId/{boardId:guid}")]
    public async Task<IActionResult> GetByBoardId(Guid boardId)
    {
        var columns = await _columnRepository.GetColumnsByBoardId(boardId);
        return Ok(columns);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ColumnsProps request)
    {
        var newColumn = await _columnRepository.CreateColumn(request);
        
        return newColumn != null 
            ? Ok(newColumn) 
            : BadRequest("Что-то пошло не так при создании колонки");
    }

    [HttpPut("moveColumn")]
    public async Task<IActionResult> Move(
        [FromQuery] Guid columnId,
        [FromQuery] int oldPosition,
        [FromQuery] int newPosition,
        [FromQuery] Guid boardId)
    {
        try
        {
            await _columnRepository.MoveAsync(columnId, oldPosition, newPosition, boardId);
            return Ok(new { message = "Column position updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { 
                message = "Failed to update column position", 
                error = ex.Message 
            });
        }
    }
    
    [HttpGet("getColumnsAndTasks/{spaceId:guid}/{boardId:guid}")]
    public async Task<IActionResult> GetColumnsWithTasks(Guid spaceId, Guid boardId)
    {
        var sql = @"
            SELECT 
                c.id AS ColumnId,
                c.title AS ColumnTitle,
                c.position AS ColumnPosition,
                t.id AS TaskId,
                t.title AS TaskTitle,
                t.board_id AS BoardId,
                t.position AS TaskPosition
            FROM columns c
            LEFT JOIN tasks t ON c.id = t.column_id
            WHERE c.board_id = @boardId
            AND EXISTS (
                SELECT 1 FROM boards b 
                WHERE b.id = @boardId 
                AND b.space_id = @spaceId
            )";

        var rows = await _db.QueryAsync<ColumnTaskRow>(sql, new { spaceId, boardId });

        var columns = rows.GroupBy(r => new { r.ColumnId, r.ColumnTitle, r.ColumnPosition })
            .OrderBy(g => g.Key.ColumnPosition)
            .Select(g => new Column
            {
                Id = g.Key.ColumnId,
                Title = g.Key.ColumnTitle,
                Position = g.Key.ColumnPosition,
                Tasks = g.Where(t => t.TaskId != null && t.TaskId != Guid.Empty)
                    .OrderBy(t => t.TaskPosition)
                    .Select(t => new TaskItem
                    {
                        Id = t.TaskId,
                        Position = t.TaskPosition,
                        Title = t.TaskTitle
                    }).ToList()
            }).ToList();

        return Ok(columns);
    }
}