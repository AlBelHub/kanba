using backend.Models;

namespace backend.Repositories.Interfaces;

public interface IColumnRepository
{
    Task<Column> CreateColumn(ColumnsProps columnsProps);
    Task<Column?> GetColumnById(int id);
    Task<IEnumerable<Column>> GetColumnsByBoardId(int boardId);
    Task<bool> UpdateColumn(int id, string title, int position);
    Task<bool> DeleteColumn(int id);
}
