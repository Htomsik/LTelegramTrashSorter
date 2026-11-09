namespace LTGTrashSorter.Core.Interfaces;

// TODO Now its unused, for future
public interface IWebSearchTool
{
    Task<string> SearchWebAsync(string query);
}