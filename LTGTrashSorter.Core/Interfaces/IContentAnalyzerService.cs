using LTGTrashSorter.Core.Models;

namespace LTGTrashSorter.Core.Interfaces;

public interface IContentAnalyzerService
{
    Task<bool> AnalyzeAndProcessNewMessagesAsync(CancellationToken cancellationToken = default);
}