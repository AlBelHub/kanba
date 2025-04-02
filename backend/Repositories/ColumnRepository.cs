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