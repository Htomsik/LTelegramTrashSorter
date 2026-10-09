namespace LTGTrashSorter.Core.Interfaces;

public interface ITelegramTools
{
    Task<string> GetChannelsList();

    Task<string> GetLastChannelsMessages();
    
    Task<string> ForwardMessage(long fromChannelId, int messageId);

    Task<string> SendTextMessage(string text);
}