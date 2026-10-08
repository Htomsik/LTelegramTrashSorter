using LTGTrashSorter.TG.Core;
using LTGTrashSorter.TG.Models;
using LTGTrashSorter.TG.Services.Interfaces;
using TL;

namespace LTGTrashSorter.TG.Services;

public class ChannelService : IChannelService
{
    private readonly BotContext _context;
    public ChannelService(BotContext context)
    {
        _context = context;
    }

    public async Task<bool> LoadLastMessagesAsync(long channelId)
    {
        if (_context.TelegramClient == null)
            return false;

        var ret = _context.Channels.TryGetValue(channelId, out var telegramChannel);
        if (!ret || telegramChannel == null)
            return false;
        
        var history = await _context.TelegramClient.Messages_GetHistory(
            peer: telegramChannel.OriginalChannel,
            limit: 10
        );
        
        if(history == null || history.Count == 0)
            return false;
        
        var telegramMessages = new  List<TelegramMessage>();
        foreach (var msgBase in history.Messages)
        {
            if (msgBase is Message msg)
            {
                telegramMessages.Add(new TelegramMessage(msg));
            }
        }

        telegramChannel.LastMessages =  telegramMessages;
        
        return true;
    }
}