using LTGTrashSorter.TG.Core;
using LTGTrashSorter.TG.Services.Interfaces;
using TL;

namespace LTGTrashSorter.TG.Services
{
    public class MessageService : IMessageService
    {
        private readonly BotContext _context;
        private readonly Func<string, string> _createMessageToUserAction;

        public MessageService(BotContext context, Func<string, string> createMessageToUserAction)
        {
            _context = context;
            _createMessageToUserAction = createMessageToUserAction;
        }
        
        public async Task SendMessageAsync(User?  targetUser, string message)
        {
            if (_context.TelegramClient is null 
                || targetUser is null  
                || targetUser.ID == 0)
                return;
            
            await _context.TelegramClient.SendMessageAsync(targetUser, message);
        }
        
        public async Task ProcessUpdatesAsync(UpdatesBase updates)
        {
            if (updates.UpdateList == null)
                return;
            
            foreach (var update in updates.UpdateList)
            {
                if (update is UpdateNewMessage unm)
                {
                    await ProcessMessageAsync(unm);
                }
            }
        }
        
        private async Task ProcessMessageAsync(UpdateNewMessage unm)
        {
            if (unm.message is not Message msg)
                return;
            
            long targetUserId = 0;
            
            if (unm.message.From == null)
            {
                {
                    targetUserId = unm.message.Peer.ID;
                }
            }
            else
            {
                targetUserId = unm.message.From.ID;
            }

            if (targetUserId == 0)
                return;
            
            // Find user in our list
            var user = _context.Users.FirstOrDefault(u => u.Id == targetUserId);
            
            // if (user == null ||
            //     !_context.AnswerUsers.Exists(x => x == user.Id))
            //     return;
            
            string messageText = msg.message;
            if (string.IsNullOrWhiteSpace(messageText))
                return;
            
            if (messageText.ToLower().StartsWith("/v"))
            {
                messageText = messageText.Replace("/v", "");
                var answer = _createMessageToUserAction.Invoke(messageText);
                await SendMessageAsync(user.OriginalUser, answer);
            }
        }
    }
} 