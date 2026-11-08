using LTGTrashSorter.Core.Models;
using LTGTrashSorter.TG.Core;
using LTGTrashSorter.TG.Models;
using LTGTrashSorter.TG.Services.Interfaces;
using Microsoft.Extensions.Options;
using TL;

namespace LTGTrashSorter.TG.Services
{
    public class DialogService : IDialogService
    {
        private readonly BotContext _context;
        private readonly Settings _settings;
        
        public DialogService(BotContext context, IOptions<Settings> settings)
        {
            _context = context;
            _settings = settings.Value;
        }
        
        public async Task<bool> LoadAsync()
        {
            if (_context.TelegramClient is null)
            {
                return false;
            }
            
            var dialogs = await _context.TelegramClient.Messages_GetAllDialogs();
            
            await LoadChannels(dialogs);
            await LoadUsers(dialogs);
            
            return true;
        }


        // Load only new channels
        private async Task<bool> LoadChannels(Messages_Dialogs? dialogs)
        {
            if (dialogs is null)
                return false;
            
            var activeChannelsIds = new HashSet<long>();
            foreach (var dialogBase in dialogs.dialogs)
            {
                if (dialogBase is Dialog { folder_id: 1 } dialog)
                    continue;

                if (dialogBase.Peer is not PeerChannel peerChannel 
                    || !dialogs.chats.TryGetValue(peerChannel.channel_id, out var channelBase)
                    || channelBase is not Channel channel)
                    continue;
                
                // Check Target channel
                if (_settings.Telegram.TargetChannelId != 0 
                    && channel.id == _settings.Telegram.TargetChannelId)
                {
                    var targetTelegramChannel = new TelegramChannel(channel);
                    _context.TargetChannel =  targetTelegramChannel;
                    continue;
                }
                
                activeChannelsIds.Add(channel.id);
                if (_context.Channels.ContainsKey(channel.id))
                {
                    continue;
                }
                
                // Add only new
                var telegramChannel = new TelegramChannel(channel);
                _context.Channels.Add(telegramChannel.Id, telegramChannel);
            }

            // Remove unsubscribed/deleted channels
            var nonExistsChannels = _context.Channels.Keys.Where(x => !activeChannelsIds.Contains(x));
            foreach (var id in nonExistsChannels)
            {
                _context.Channels.Remove(id);
            }
            
            return true;
        }
        
        // Load All users
        private async Task<bool> LoadUsers(Messages_Dialogs? dialogs)
        {
            if (dialogs is null)
                return false;
            
            _context.Users.Clear();
            foreach (var dialogBase in dialogs.dialogs)
            {
                if (dialogBase is Dialog { folder_id: 1 } dialog)
                    continue;

                if (dialogBase.Peer is not PeerUser peerUser || !dialogs.users.TryGetValue(peerUser.user_id, out var user)) 
                    continue;
                
                var telegramUser = new TelegramUser(user);
                _context.Users[telegramUser.Id] = telegramUser;
            }
            
            return true;
        }
    }
}