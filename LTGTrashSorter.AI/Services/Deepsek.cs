using System.ClientModel;
using LTGTrashSorter.AI.Tools;
using Microsoft.Extensions.AI;
using OpenAI;

namespace LTGTrashSorter.AI.Services;

public class DeepSeekService : IAiService
{
    private readonly IEnumerable<AITool>? _tools;
    private readonly IChatClient _chatClient;

    public DeepSeekService(string apiKey, string baseUrl = "https://api.deepseek.com",
        IEnumerable<AITool>? tools = null)
    {
        _tools = tools;
        var openAiOptions = new OpenAIClientOptions
        {
            Endpoint = new Uri(baseUrl)
        };

        var openAiClient = new OpenAIClient(new ApiKeyCredential(apiKey), openAiOptions);
        
        _chatClient = openAiClient.GetChatClient("deepseek-chat")
            .AsIChatClient()
            .AsBuilder()
            .UseFunctionInvocation() // Авто-вызов C# функций
            .Build();
    }
    
    public async Task<string> ProcessMessageAsync(string systemPrompt, string userMessage)
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, userMessage)
        };
        
        
        var options = new ChatOptions
        {
            Temperature = 0.5f, 
            ResponseFormat = ChatResponseFormat.Json,
            Tools = _tools?.ToList()
            
        };
        
        var response = await _chatClient.GetResponseAsync(messages, options);
        return response.Text ?? string.Empty;
    }
}