namespace LTGTrashSorter.DB.Models;

public class TelegramChannelEntity
{
    public long Id { get; set; }
    
    public int LastProcessedMessageId { get; set; }
}