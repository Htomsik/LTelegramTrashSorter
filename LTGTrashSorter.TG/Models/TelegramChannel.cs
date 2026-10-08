using Newtonsoft.Json;
using TL;

namespace LTGTrashSorter.TG.Models;

public record TelegramChannel
{
    public long Id { get; init; }
    
    public string DisplayName {get; init;}
    
    public int SubscribersCount { get; init; }
    
    [JsonIgnore]
    public Channel OriginalChannel { get; init; }
    
    public bool IsGroup { get; init; } 
    
    public bool IsChannel { get; init; } 
    
    public TelegramChannel(Channel channel)
    {
        Id = channel.ID;
        OriginalChannel =  channel;
        DisplayName = channel.Title ?? string.Empty;
        SubscribersCount = channel.participants_count;
        IsChannel = channel.IsChannel;
        IsGroup = channel.IsGroup;
    }
}