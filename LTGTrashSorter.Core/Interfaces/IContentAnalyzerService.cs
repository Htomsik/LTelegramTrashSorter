using LTGTrashSorter.Core.Models;

namespace LTGTrashSorter.Core.Interfaces;

public interface IContentAnalyzerService
{
    Task<ContentAnalyzerResult> AnalyzeAndProcessNewMessagesAsync(CancellationToken cancellationToken = default);
}