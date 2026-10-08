namespace LTGTrashSorter.Core.Interfaces;

public interface ITelegramTools
{
    Task<string> GetProcessChannelsList();
}