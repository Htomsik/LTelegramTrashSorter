using LTGTrashSorter.TG.Models;
using TL;
using WTelegram;

namespace LTGTrashSorter.TG.Core
{
    public class BotContext 
    {
        /// <summary>
        ///     PERMANENT TG Authorization dataset
        /// </summary>
        public AuthorizationData AuthorizationData { get; set; }
        
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
        public List<TelegramUser> Users { get; set; } = new();
        
        /// <summary>
        ///     Contact list to answer
        /// </summary>
        public List<long> AnswerUsers { get; set; } = new();
        
        public BotContext(AuthorizationData authorizationData)
        {
            AuthorizationData = authorizationData;
        }
    }
}