using System.Text.Json.Serialization;

namespace LTGTrashSorter.Core.Models;

public class Settings
{
    public WorkerSettings Worker { get; set; } = new();
        
    public TelegramSettings Telegram { get; set; } = new();
    
    public OpenAiSettings Ai { get; set; } = new();

    public DbSettings DB { get; set; } = new();
    
}
public class WorkerSettings
{
    public int[] ExecutionHours { get; set; }
    
    public string TimeZoneId { get; set; } = "Europe/Moscow";
    
    [JsonIgnore]
    public int DefaultTimeout { get; set; } = 14400000;
    
    private TimeZoneInfo? _timeZone;

    [JsonIgnore]
    public TimeZoneInfo TimeZone => _timeZone ??= TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
}

public class DbSettings
{
    public string DbPath { get; set; } = "database.db";
}

public class TelegramSettings
{
    public string ApiId { get; set; }
    
    public string ApiHash { get; set; } = string.Empty;
    
    public string PhoneNumber { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty;
    
    public string Proxy {get; set;} = string.Empty;

    public int LastMessagesProcessLimit { get; set; } = 5;
    
    /// <summary>
    ///     Target channel for posting 
    /// </summary>
    public long TargetChannelId { get; set; }
    
    public string TargetChannelLinkText { get; set; }
    
    public string TargetChannelLinkUrl { get; set; }
    
    /// <summary>
    ///     Log channel, posting messages from analyzer 
    /// </summary>
    public long LogChannelId { get; set; }
    
    public string SessionPath { get; set; } = "WTelegram.session";
}

public class OpenAiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    
    public string WebToolKey { get; set; } = string.Empty;

    public string BaseURL { get; set; } = string.Empty;

    public string ModelName { get; set; } = string.Empty;
    
    public string[] SystemPrompt { get; set; } = Array.Empty<string>();
    
    public string GetFormattedSystemPrompt() => string.Join("\n", SystemPrompt);
}