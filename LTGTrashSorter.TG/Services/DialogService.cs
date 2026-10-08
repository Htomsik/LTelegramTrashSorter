using LTGTrashSorter.TG.Core;
using LTGTrashSorter.TG.Models;
using LTGTrashSorter.TG.Services.Interfaces;
using TL;

namespace LTGTrashSorter.TG.Services
{
    public class DialogService : IDialogService
    {
        private readonly BotContext _context;
        
        public DialogService(BotContext context)
        {
            _context = context;
        }
        
        public async Task<bool> LoadAsync()
        {
            if (_context.TelegramClient is null)
            {
                return false;
            }
            
            var dialogs = await _context.TelegramClient.Messages_GetAllDialogs();
            
            var oldSelectedUsers = _context.ProcessUsers.ToList();
            var oldSelectedChannels = _context.ProcessChannels.ToList();
            
            _context.Users.Clear();
            
            // Add all dialogs 
            foreach (var dialog in dialogs.dialogs)
            {
                //TODO Add mapper only if needed
                switch (dialog.Peer)
                {
                    case PeerUser peerUser:
                        var user = dialogs.users[peerUser.user_id];
                        var telegramUser = new TelegramUser(user);
                        _context.Users.Add(telegramUser);
                        break;
                    
                    case PeerChannel peerChannel:
                        var channelBase = dialogs.chats[peerChannel.channel_id];
                        if (channelBase is Channel channel)
                        {
                            var telegramChannel = new TelegramChannel(channel);
                            _context.Channels.Add(telegramChannel);
                        }
                        break;
                }
            }
            
            // Restore Selection
            foreach (var selectId in oldSelectedUsers)
            {
                if (_context.Users.Any(x => x.Id == selectId))
                {
                    _context.ProcessUsers.Add(selectId);
                }
            }
            
            foreach (var selectId in oldSelectedChannels)
            {
                if (_context.Channels.Any(x => x.Id == selectId))
                {
                    _context.ProcessUsers.Add(selectId);
                }
            }
            
            return true;
        }
    }
}