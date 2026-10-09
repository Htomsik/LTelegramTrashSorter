namespace LTGTrashSorter.Core.Interfaces;

public interface IWebSearchTool
{
    Task<string> SearchWebAsync(string query);
}