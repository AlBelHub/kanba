using backend.Models;

namespace backend.Repositories.Interfaces;

public interface IColumnRepository
{
    Task<Column> CreateColumn(ColumnsProps columnsProps);
    Task<Column?> GetColumnById(Guid id);
    Task<IEnumerable<Column>> GetColumnsByBoardId(Guid boardId);
    Task<bool> UpdateColumn(Guid id, string title, Guid position);
    Task<bool> DeleteColumn(Guid id);
    Task<bool> MoveAsync(Guid ColumnId, int OldPosition, int NewPosition, Guid BoardId);
}
