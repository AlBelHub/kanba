using System.Data;
using backend.Models;
using backend.Repositories.Abstract;
using backend.Repositories.Interfaces;

namespace backend.Repositories;

public class ColumnRepository : RepositoryBase, IColumnRepository
{
    public ColumnRepository(IDbConnection db) : base(db)
    {
    }

    public async Task<Column> CreateColumn(ColumnsProps columnsProps)
    {
        throw new NotImplementedException();
    }

    public async Task<Column?> GetColumnById(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Column>> GetColumnsByBoardId(int boardId)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> UpdateColumn(int id, string title, int position)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteColumn(int id)
    {
        throw new NotImplementedException();
    }
}