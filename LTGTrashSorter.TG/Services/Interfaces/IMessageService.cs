using TL;

namespace LTGTrashSorter.TG.Services.Interfaces
{
    public interface IMessageService
    {
        /// <summary>
        ///     Send text message to user
        /// </summary>
        Task SendMessageAsync(User targetUser, string message);

        
        /// <summary>
        ///     Background message worker
        /// </summary>
        Task ProcessUpdatesAsync(UpdatesBase updates);
    }
}