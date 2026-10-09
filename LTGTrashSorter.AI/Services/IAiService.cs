namespace LTGTrashSorter.AI.Services;

public interface IAiService
{
    Task<string> ProcessMessageAsync(string userMessage);
}