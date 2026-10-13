using System.Text.RegularExpressions;
using LTGTrashSorter.TG.Core;
using LTGTrashSorter.TG.Services.Interfaces;
using TL;
using WTelegram;

namespace LTGTrashSorter.TG.Services
{
    public class MessageService : IMessageService
    {
        private readonly BotContext _context;

        public MessageService(BotContext context)
        {
            _context = context;
        }
        
        public async Task SendMessageAsync(long targetId, string message)
        {
            var targetPeer = _context.GetInputPeer(targetId);
            
            if (_context.TelegramClient is null 
                || targetPeer is null  
                || targetPeer.ID == 0)
                return;
            
            await _context.TelegramClient.SendMessageAsync(targetPeer, message);
        }

        public async Task<bool> ForwardMessageByIdsAsync(long fromId, long toId, int messageId)
        {
            if (_context.TelegramClient is null) return false;
            
            var fromPeer = _context.GetInputPeer(fromId);
            var toPeer = _context.GetInputPeer(toId);

            if (fromPeer is null || toPeer is null) 
                return false;

            await _context.TelegramClient.Messages_ForwardMessages(
                from_peer: fromPeer,
                id: new[] { messageId },
                random_id: new[] { Helpers.RandomLong() },
                to_peer: toPeer
            );

            return true;
        }
    }
} 