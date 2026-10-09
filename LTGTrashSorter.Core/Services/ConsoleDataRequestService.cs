using LTGTrashSorter.Core.Interfaces;

namespace LTGTrashSorter.Core.Services;

public class ConsoleDataRequestService : IDataRequestService
{
    public string GetData(string key)
    {
        Console.WriteLine("Key requested: " + key);
        return Console.ReadLine();
    }
}