using LTGTrashSorter.TG.Core;
using LTGTrashSorter.TG.Models;
using LTGTrashSorter.TG.Services.Interfaces;
using TL;

namespace LTGTrashSorter.TG.Services
{
    public class UserService : IUserService
    {
        private readonly BotContext _context;
        
        public UserService(BotContext context)
        {
            _context = context;
        }
        
        public async Task LoadUsersAsync()
        {
            var dialogs = await _context.TelegramClient.Messages_GetAllDialogs();
            
            var oldSelectedUsers = _context.AnswerUsers.ToList();
            
            _context.Users.Clear();
            
            // Add all users from dialogs
            foreach (var dialog in dialogs.dialogs)
            {
                if (dialog.Peer is not PeerUser peerUser)
                    continue;
                
                var user = dialogs.users[peerUser.user_id];
                var telegramUser = new TelegramUser(user);
                
                _context.Users.Add(telegramUser);
            }
            
            // Restore Selection
            foreach (var selectId in oldSelectedUsers)
            {
                if (_context.Users.Any(x => x.Id == selectId))
                {
                    _context.AnswerUsers.Add(selectId);
                }
            }
        }
    }
}