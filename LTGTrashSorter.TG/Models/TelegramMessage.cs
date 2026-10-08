using TL;

namespace LTGTrashSorter.TG.Models;

public record TelegramMessage
{
    public long Id { get; init; }
    
    public long PeerId { get; init; }
    
    public string Message { get; init; } = "";

    public TelegramMessage(Message msg)
    {
        Message = msg.message;
        PeerId = msg.peer_id.ID;
        Id = msg.ID;
    }
}