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

    public async Task<IEnumerable<TelegramMessage>> LoadLastMessagesAsync(long channelId)
    {
        if(_context.TelegramClient == null)
            return new List<TelegramMessage>();
        
        var telegramChannel = _context.Channels.FirstOrDefault(c => c.Id == channelId);
        if (telegramChannel == null)
            return new List<TelegramMessage>();
        
        var history = await _context.TelegramClient.Messages_GetHistory(
            peer: telegramChannel.OriginalChannel,
            limit: 10
        );
          
        var telegramMessages = new  List<TelegramMessage>();
        foreach (var msgBase in history.Messages)
        {
            if (msgBase is Message msg)
            {
                telegramMessages.Add(new TelegramMessage(msg));
            }
        }
        return telegramMessages;
    }
}