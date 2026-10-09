using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using LTGTrashSorter.Core.Interfaces;
using LTGTrashSorter.Core.Models;
using Microsoft.Extensions.Options;

namespace LTGTrashSorter.AI.Tools;

public class SearchTools : IWebSearchTool
{
    private readonly HttpClient _httpClient = new();
    private readonly Settings _settings;
    
    public SearchTools(IOptions<Settings>  settings)
    {
        _settings =  settings.Value;
    }

    [Description("Название: Поиск информации Функция:Ищет актуальную информацию, точное время, свежие новости и факты в Google.")]
    public async Task<string> SearchWebAsync(
        [Description("Поисковый запрос на естественном языке")] string query)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://google.serper.dev/search");
            request.Headers.Add("X-API-KEY", _settings.Ai.WebToolKey);
            request.Content = JsonContent.Create(new 
            { 
                q = query, 
                gl = "ru", 
                hl = "ru" 
            });

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return "Не удалось получить результаты поиска от Serper.";

            var result = await response.Content.ReadFromJsonAsync<SerperResponse>();
            
            if (!string.IsNullOrWhiteSpace(result?.AnswerBox?.Answer))
                return result.AnswerBox.Answer;

            if (!string.IsNullOrWhiteSpace(result?.AnswerBox?.Snippet))
                return result.AnswerBox.Snippet;
            
            if (result?.Organic == null || result.Organic.Count == 0)
                return "Ничего не найдено в интернете.";

            var snippets = result.Organic
                .Take(4)
                .Select(r => $"- {r.Title}: {r.Snippet}");

            return string.Join("\n", snippets);
        }
        catch (Exception ex)
        {
            return $"Ошибка при поиске через Serper: {ex.Message}";
        }
    }

    // DTO
    private record SerperResponse(
        [property: JsonPropertyName("answerBox")] SerperAnswerBox? AnswerBox,
        [property: JsonPropertyName("organic")] List<SerperOrganic>? Organic
    );

    private record SerperAnswerBox(
        [property: JsonPropertyName("answer")] string? Answer,
        [property: JsonPropertyName("snippet")] string? Snippet
    );

    private record SerperOrganic(
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("snippet")] string Snippet
    );
}