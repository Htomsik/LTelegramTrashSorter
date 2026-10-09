using System.ClientModel;
using LTGTrashSorter.AI.Tools;
using LTGTrashSorter.Core.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;

namespace LTGTrashSorter.AI.Services;

public class DeepSeekService : IAiService
{
    private readonly IEnumerable<AITool>? _tools;
    private readonly IChatClient _chatClient;
    
    private readonly Settings _settings;

    public DeepSeekService(IOptions<Settings> settings,
        IEnumerable<AITool>? tools = null)
    {
        _settings = settings.Value;
        _tools = tools;
        
        var openAiOptions = new OpenAIClientOptions
        {
            Endpoint = new Uri(_settings.Ai.BaseURL)
        };

        var openAiClient = new OpenAIClient(new ApiKeyCredential(_settings.Ai.ApiKey), openAiOptions);
        _chatClient = openAiClient.GetChatClient(_settings.Ai.ModelName)
            .AsIChatClient()
            .AsBuilder()
            .UseFunctionInvocation() // Авто-вызов C# функций
            .Build();
    }
    
    public async Task<string> ProcessMessageAsync( string userMessage)
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, _settings.Ai.SystemPrompt),
            new(ChatRole.User, userMessage)
        };
        
        var options = new ChatOptions
        {
            Temperature = 0.1f, 
            ResponseFormat = ChatResponseFormat.Json,
            Tools = _tools?.ToList(),
            ToolMode = ChatToolMode.Auto
        };
        
        var response = await _chatClient.GetResponseAsync(messages, options);
        return response.Text ?? string.Empty;
    }
}