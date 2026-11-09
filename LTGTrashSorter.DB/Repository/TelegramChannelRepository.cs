using Dapper;
using LTGTrashSorter.Core.Models;
using LTGTrashSorter.DB.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace LTGTrashSorter.DB.Repository;

public interface ITelegramChannelRepository
{
    Task<IEnumerable<TelegramChannelEntity>> GetAllAsync();
    Task SaveAsync(TelegramChannelEntity channel);
}

public class TelegramChannelRepository : ITelegramChannelRepository
{
    private readonly DbSettings _dbSettings;
    
    private string ConnectionString => $"Data Source={_dbSettings.DbPath}";

    public TelegramChannelRepository(IOptions<Settings> options)
    {
        _dbSettings = options.Value.DB;
    }

    public async Task<IEnumerable<TelegramChannelEntity>> GetAllAsync()
    {
        using var connection = new SqliteConnection(ConnectionString);
        return await connection.QueryAsync<TelegramChannelEntity>(
            "SELECT * FROM TelegramChannels"
        );
    }

    public async Task SaveAsync(TelegramChannelEntity channel)
    {
        using var connection = new SqliteConnection(ConnectionString);
        const string sql = @"
            INSERT INTO TelegramChannels (Id, LastProcessedMessageId) 
            VALUES (@Id, @LastProcessedMessageId)
            ON CONFLICT(Id) DO UPDATE SET 
                LastProcessedMessageId = excluded.LastProcessedMessageId;
        ";

        await connection.ExecuteAsync(sql, channel); // Dapper сам промаппит свойства объекта на параметры SQL
    }
}