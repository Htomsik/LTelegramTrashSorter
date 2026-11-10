using LTGTrashSorter.AI.Services;
using LTGTrashSorter.Core.Interfaces;
using LTGTrashSorter.Core.Models;
using LTGTrashSorter.Core.Models.DTO;
using LTGTrashSorter.TG.Core;
using Newtonsoft.Json;

namespace LTGTrashSorter.Worker.Services;


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
    
    public async Task<ContentAnalyzerResult> AnalyzeAndProcessNewMessagesAsync(CancellationToken cancellationToken = default)
    {
        var unanalyzedMessagesCount = _botContext.Channels.Values
            .Sum(c => c.LastMessages.Count);

        if (unanalyzedMessagesCount == 0)
        {
            _logger.LogInformation("No new messages found.");
            return new ContentAnalyzerResult("No new messages found.", false);
        }

        string prompt = $"Проанализируй последние полученные сообщения из каналов. " +
                        $"Если найдешь действительно важные посты — перешли их с помощью инструмента пересылки. " +
                        $"Если ничего интересного нет — ничего не пересылай.";

        _logger.LogInformation("Send request to analyze message (Messages: {Count})...", unanalyzedMessagesCount);
        var responseJson = await _aiService.ProcessMessageAsync(prompt, cancellationToken);
        _logger.LogInformation("Analyzer response: {response}", responseJson);
        
        string messageText = ExtractMessageFromJson(responseJson);
        
        ClearProcessedMessages();
        return new  ContentAnalyzerResult(messageText, false, _botContext.Channels.Count, unanalyzedMessagesCount);
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
    
    private string ExtractMessageFromJson(string jsonResponse)
    {
        if (string.IsNullOrWhiteSpace(jsonResponse))
            return "Пустой ответ от AI.";

        try
        {
            var parsed = JsonConvert.DeserializeObject<AnalyzerResponseDTO>(jsonResponse);
            if (parsed != null && !string.IsNullOrEmpty(parsed.Message))
            {
                return parsed.Message;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Can't parse response: {jsonResponse}", jsonResponse);
        }
        
        return jsonResponse;
    }
}