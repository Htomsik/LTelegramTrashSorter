using TL;

namespace LTGTrashSorter.TG.Services.Interfaces
{
    public interface IMessageService
    {
        /// <summary>
        ///     Send text message 
        /// </summary>
        Task SendMessageAsync(long targetId, string message);
        
        /// <summary>
        ///     Send text message with link footer
        /// </summary>
        Task SendMessageWithLinkAsync(long targetId, string message, string linkText, string url);
        
        /// <summary>
        ///     Forward message from user/chat/group
        /// </summary>
        Task<bool> ForwardMessageByIdsAsync(long fromId, long toId, int messageId, bool clearPeerData = false, bool isSilent = false);
        
    }
}