using LTGTrashSorter.Core.Models.DTO;

namespace LTGTrashSorter.Core.Interfaces;

public interface ITelegramTools
{
    Task<string> GetChannelsList();

    Task<string> GetLastChannelsMessages();
    
    Task<string> ForwardMessage(List<TelegramImportantMessageDto>  importantMessages);

    Task<string> SendTextMessage(string text);
}