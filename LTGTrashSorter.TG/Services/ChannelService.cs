using LTGTrashSorter.Core.Models;
using LTGTrashSorter.TG.Core;
using LTGTrashSorter.TG.Models;
using LTGTrashSorter.TG.Services.Interfaces;
using Microsoft.Extensions.Options;
using TL;

namespace LTGTrashSorter.TG.Services;

public class ChannelService : IChannelService
{
    private readonly BotContext _context;
    private readonly Settings _settings;
    public ChannelService(BotContext context, IOptions<Settings>  settings)
    {
        _context = context;
        _settings = settings.Value;
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
            limit: _settings.Telegram.LastMessagesSaveLimit
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