using System.Data;
using Dapper;

namespace backend.Repositories.Abstract;

public abstract class RepositoryBase
{
    protected readonly IDbConnection _db;

    protected RepositoryBase(IDbConnection db)
    {
        _db = db;
    }

    // Выполнение запроса, возвращающего коллекцию данных
    protected async Task<IEnumerable<T>> QueryAsync<T>(string commandText, object parameters = null)
    {
        return await _db.QueryAsync<T>(commandText, parameters);
    }

    // Выполнение запроса и возврат первого результата или null
    protected async Task<T> QueryFirstOrDefaultAsync<T>(string commandText, object parameters = null)
    {
        return await _db.QueryFirstOrDefaultAsync<T>(commandText, parameters);
    }

    // Выполнение запроса и возврат одиночного значения
    protected async Task<T> ExecuteScalarAsync<T>(string commandText, object parameters = null)
    {
        return await _db.ExecuteScalarAsync<T>(commandText, parameters);
    }

    // Выполнение команды без возвращаемых данных
    protected async Task ExecuteAsync(string commandText, object parameters = null)
    {
        await _db.ExecuteAsync(commandText, parameters);
    }
}