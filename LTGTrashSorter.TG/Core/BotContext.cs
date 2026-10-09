using LTGTrashSorter.TG.Models;
using TL;
using WTelegram;

namespace LTGTrashSorter.TG.Core
{
    public class BotContext 
    {
        /// <summary>
        ///     Api client for TG operations
        /// </summary>
        public Client? TelegramClient { get; set; }
        
        /// <summary>
        ///     CurrentUser
        /// </summary>
        public User? Me { get; set; }
        
        /// <summary>
        ///     Contact list
        /// </summary>
        public Dictionary<long, TelegramUser> Users { get; set; } = new();
        
        /// <summary>
        ///    Subscribed channels
        /// </summary>
        public Dictionary<long, TelegramChannel> Channels { get; set; } = new();
        
        public InputPeer? GetInputPeer(long id)
        {
            var ret = Users.TryGetValue(id, out var user);
            if (ret && user is  not null)
            {
                return user.OriginalUser.ToInputPeer();
            }
            
            ret = Channels.TryGetValue(id, out var channel);
            if (ret && channel is  not null)
            {
                return channel.OriginalChannel.ToInputPeer();
            }
            
            return null;
        }
    }
}