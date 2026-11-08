using LTGTrashSorter.TG.Models;

namespace LTGTrashSorter.TG.Services.Interfaces;

public interface IChannelService
{
    public Task<bool> LoadLastMessagesAsync(long channelId, int minMessageId = 0);
}