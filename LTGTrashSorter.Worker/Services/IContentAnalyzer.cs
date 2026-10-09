using LTGTrashSorter.AI.Services;
using LTGTrashSorter.TG.Core;

namespace LTGTrashSorter.Worker.Services;

public interface IContentAnalyzerService
{
    Task AnalyzeAndProcessNewMessagesAsync(CancellationToken cancellationToken = default);
}

public class ContentAnalyzerService : IContentAnalyzerService
{
    private readonly BotContext _botContext;
    private readonly IAiService _aiService;
    private readonly ILogger<ContentAnalyzerService> _logger;

    public ContentAnalyzerService(
        BotContext botContext,
        IAiService aiService,
        ILogger<ContentAnalyzerService> logger)
    {
        _botContext = botContext;
        _aiService = aiService;
        _logger = logger;
    }
    
    public async Task AnalyzeAndProcessNewMessagesAsync(CancellationToken cancellationToken = default)
    {
        var unanalyzedMessagesCount = _botContext.Channels.Values
            .Sum(c => c.LastMessages.Count);

        if (unanalyzedMessagesCount == 0)
        {
            _logger.LogInformation("No new messages found.");
            return;
        }
        
        string prompt = $"Проанализируй последние полученные сообщения из каналов. " +
                        $"Если найдешь действительно важные посты — перешли их с помощью инструмента ForwardMessage. " +
                        $"Если ничего интересного нет — ничего не пересылай.";

        _logger.LogInformation("Send request to analyze message (Messages: {Count})...", unanalyzedMessagesCount);
        
        await _aiService.ProcessMessageAsync(prompt, cancellationToken);
        
        ClearProcessedMessages();
    }

    private void ClearProcessedMessages()
    {
        foreach (var channel in _botContext.Channels.Values)
        {
            channel.LastMessages.Clear(); // Или помечаем IsProcessed = true
        }
    }
}