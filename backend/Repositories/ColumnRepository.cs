using System.Data;
using backend.Helpers;
using backend.Models;
using backend.Repositories.Abstract;
using backend.Repositories.Interfaces;
using Dapper;

namespace backend.Repositories;

public class ColumnRepository : RepositoryBase, IColumnRepository
{

    private readonly IUUIDProvider _uuidProvider;

    public ColumnRepository(IDbConnection db, IUUIDProvider uuidProvider) : base(db)
    {
        _uuidProvider = uuidProvider;
    }

    public async Task<Column> CreateColumn(ColumnsProps columnsProps)
    {

        var id = _uuidProvider.GenerateUUIDv7();

        string sql = @"
        INSERT INTO Columns (id, board_id, title, created_by, position) 
        VALUES (@id, @BoardId, @Title, @CreatedBy, @Position)
        RETURNING *";

        return await _db.QueryFirstOrDefaultAsync<Column>(sql, new
        {
            id = id,
            BoardId = columnsProps.BoardId,
            Title = columnsProps.Title,
            CreatedBy = columnsProps.CreatedBy,
            Position = columnsProps.Position
        });
    }

    public Task<Column?> GetColumnById(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Column>> GetColumnsByBoardId(Guid boardId)
    {
        var columnByBoardId = await _db.QueryAsync<Column>(
            "SELECT * FROM columns WHERE board_id = @BoardId",
            new { BoardId = boardId });
        return columnByBoardId;
    }

    public Task<bool> UpdateColumn(Guid id, string title, Guid position)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateColumn(Guid id, string title, int position)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteColumn(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> MoveAsync(Guid ColumnId, int OldPosition, int NewPosition, Guid BoardId)
    {
        if (_db.State != ConnectionState.Open)
        {
            _db.Open();
        }

        using var transaction = _db.BeginTransaction();

        try
        {
            // 1. Проверяем, существует ли колонка с заданным ColumnId
            var columnExists = await _db.ExecuteScalarAsync<bool>(
                "SELECT EXISTS (SELECT 1 FROM columns WHERE id = @ColumnId AND board_id = @BoardId)",
                new { ColumnId, BoardId });

            if (!columnExists)
            {
                //return NotFound(new { message = "Column not found in the specified board" });
                return false;
            }

            // 2. Если колонка переместилась вниз
            if (OldPosition < NewPosition)
            {
                await _db.ExecuteAsync(
                    "UPDATE columns SET position = position - 1 WHERE position > @OldPosition AND position <= @NewPosition AND board_id = @BoardId",
                    new { OldPosition, NewPosition, BoardId }, transaction);
            }
            // 3. Если колонка переместилась вверх
            else if (OldPosition > NewPosition)
            {
                await _db.ExecuteAsync(
                    "UPDATE columns SET position = position + 1 WHERE position < @OldPosition AND position >= @NewPosition AND board_id = @BoardId",
                    new { OldPosition, NewPosition, BoardId }, transaction);
            }

            // 4. Обновляем позицию перемещённой колонки
            await _db.ExecuteAsync(
                "UPDATE columns SET position = @NewPosition WHERE id = @ColumnId AND board_id = @BoardId",
                new { ColumnId, NewPosition, BoardId }, transaction);

            transaction.Commit();
            //return Results.Ok(new { message = "Column position updated successfully" });
            return true;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            //return Results.BadRequest(new { message = "Failed to update column position", error = ex.Message });
            return false;
        }
    }
}