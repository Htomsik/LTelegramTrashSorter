using LTGTrashSorter.Core.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LTGTrashSorter.DB.Infrastructure;

public class DbInitializer
{
    private readonly ILogger<DbInitializer> _logger;
    private readonly DbSettings _settings;

    public DbInitializer(ILogger<DbInitializer> logger, IOptions<Settings> options)
    {
        _logger = logger;
        _settings = options.Value.DB;
    }

    public void Initialize()
    {
        try
        {
            _logger.LogInformation("Initializing SQLite database at path: {DbPath}", _settings.DbPath);
            
            var dir = Path.GetDirectoryName(_settings.DbPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                _logger.LogInformation("Created directory for database: {Directory}", dir);
            }

            // EF too big for this, dapper - better
            using var connection = new SqliteConnection($"Data Source={_settings.DbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE IF NOT EXISTS TelegramChannels (Id INTEGER PRIMARY KEY, LastProcessedMessageId INTEGER NOT NULL);";
            command.ExecuteNonQuery();

            _logger.LogInformation("Database initialized successfully.");
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Failed to initialize database: {Message}", e.Message);
            throw;
        }
    }
}