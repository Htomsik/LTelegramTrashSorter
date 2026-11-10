namespace LTGTrashSorter.Core.Interfaces;

public interface ITelegramStateService
{
    Task<bool> LoadStateIntoContextAsync();
    
    Task<bool> SaveStateFromContextAsync();
}