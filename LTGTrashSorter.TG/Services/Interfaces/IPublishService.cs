using LTGTrashSorter.Core.Models.DTO;

namespace LTGTrashSorter.TG.Services.Interfaces;

public interface IPublishService
{
    void AddMessage(TelegramImportantMessageDto message);
    
    Task PublishBatchAsync(int channelsCount, int messagesCount, DateTimeOffset publishedAt);
}