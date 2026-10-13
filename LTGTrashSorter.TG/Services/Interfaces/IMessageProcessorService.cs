using TL;

namespace LTGTrashSorter.TG.Services.Interfaces;

public interface IMessageProcessorService
{
    /// <summary>
    ///     Background message subscribe
    /// </summary>
    Task ProcessUpdatesAsync(UpdatesBase updates);
    
    /// <summary>
    ///     Respond to user messages
    /// </summary>
    public Func<string, string>? CreateMessageToUserAction { get; set; }
}