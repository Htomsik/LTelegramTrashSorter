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
        
        public async Task SendMessageWithLinkAsync(long targetId, string message, string linkText, string url)
        {
            var targetPeer = _context.GetInputPeer(targetId);
    
            if (_context.TelegramClient is null 
                || targetPeer is null  
                || targetPeer.ID == 0)
                return;

            MessageEntity[]? entities = null;

            if (!string.IsNullOrEmpty(linkText) && !string.IsNullOrEmpty(url))
            {
                int index = message.IndexOf(linkText, StringComparison.Ordinal);
                if (index != -1)
                {
                    entities = new MessageEntity[]
                    {
                        new MessageEntityTextUrl()
                        {
                            offset = index,
                            length = linkText.Length,
                            url = url
                        }
                    };
                }
            }

            await _context.TelegramClient.Messages_SendMessage(
                peer: targetPeer,
                message: message,
                random_id: WTelegram.Helpers.RandomLong(),
                entities: entities,
                no_webpage: true // Отключаем предпросмотр
            );
        }

        public async Task<bool> ForwardMessageByIdsAsync(long fromId, long toId, int messageId, bool clearPeerData = false, bool isSilent = false)
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
                to_peer: toPeer,
                drop_author: clearPeerData,
                silent: isSilent
                
            );

            return true;
        }
    }
} 