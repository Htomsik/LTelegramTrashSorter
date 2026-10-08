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
            
            _context.Users.Clear();
            _context.Channels.Clear();
            
            // Add all dialogs 
            foreach (var dialogBase in dialogs.dialogs)
            {
                // Scip Archive
                if (dialogBase is Dialog dialog)
                {
                    if (dialog.folder_id == 1)
                        continue;
                }
                
                //TODO Add mapper only if needed
                switch (dialogBase.Peer)
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
            
            
            return true;
        }
    }
}