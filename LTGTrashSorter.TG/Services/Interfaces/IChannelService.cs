using LTGTrashSorter.TG.Models;

namespace LTGTrashSorter.TG.Services.Interfaces;

public interface IChannelService
{
    public Task<IEnumerable<TelegramMessage>> LoadLastMessagesAsync(long channelId);
}