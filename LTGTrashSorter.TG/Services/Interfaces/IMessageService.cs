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
        ///     Forward message from user/chat/group
        /// </summary>
        Task<bool> ForwardMessageByIdsAsync(long fromId, long toId, int messageId);
        
        /// <summary>
        ///     Background message worker
        /// </summary>
        Task ProcessUpdatesAsync(UpdatesBase updates);
    }
}