using LTGTrashSorter.AI.Services;
using LTGTrashSorter.TG.Core;

namespace LTGTrashSorter.Worker.Services;

public interface IContentAnalyzerService
{
    Task<bool> AnalyzeAndProcessNewMessagesAsync(CancellationToken cancellationToken = default);
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
    
    public async Task<bool> AnalyzeAndProcessNewMessagesAsync(CancellationToken cancellationToken = default)
    {
        var unanalyzedMessagesCount = _botContext.Channels.Values
            .Sum(c => c.LastMessages.Count);

        if (unanalyzedMessagesCount == 0)
        {
            _logger.LogInformation("No new messages found.");
            return true;
        }
        
        string prompt = $"Проанализируй последние полученные сообщения из каналов. " +
                        $"Если найдешь действительно важные посты — перешли их с помощью инструмента пересылки. " +
                        $"Если ничего интересного нет — ничего не пересылай." +
                        $"Сейчас ты работаешь в тестовом режиме, поэтому пересылай хотя бы одну новость.";

        _logger.LogInformation("Send request to analyze message (Messages: {Count})...", unanalyzedMessagesCount);
        var response = await _aiService.ProcessMessageAsync(prompt, cancellationToken);
        _logger.LogInformation("Analyzer response: {response}", response);
        
        ClearProcessedMessages();
        return true;
    }

    private void ClearProcessedMessages()
    {
        foreach (var channel in _botContext.Channels.Values)
        {
            // Save last process message for next analyzing
            if (channel.LastMessages.Count > 0)
            {
                channel.LastProcessedMessageId = channel.LastMessages.Max(m => m.Id);
            }
            
            channel.LastMessages.Clear();
        }
    }
}