using Newtonsoft.Json;
using TL;

namespace LTGTrashSorter.TG.Models;

public record TelegramChannel
{
    public long Id { get; init; }
    
    public string DisplayName {get; init;}
    
    public int SubscribersCount { get; init; }
    
    public bool IsTargetChannel {get; init;}
    
    [JsonIgnore]
    public int LastProcessedMessageId { get; set; } = 0;
    
    [JsonIgnore]
    public Channel OriginalChannel { get; init; }
    
    [JsonIgnore]
    public List<TelegramMessage> LastMessages {get; set; }
    
    public TelegramChannel(Channel channel, bool isTargetChannel = false)
    {
        Id = channel.ID;
        OriginalChannel =  channel;
        DisplayName = channel.Title ?? string.Empty;
        SubscribersCount = channel.participants_count;
        IsTargetChannel = isTargetChannel;
    }
}
