using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace LTGTrashSorter.AI.Tools;

public class SearchTools
{
    private readonly HttpClient _httpClient = new();
    private const string ApiKey = ""; // Вставьте сюда ключ из кабинета Serper.dev

    [Description("Название: Поиск информации Функция:Ищет актуальную информацию, точное время, свежие новости и факты в Google.")]
    public async Task<string> SearchWebAsync(
        [Description("Поисковый запрос на естественном языке")] string query)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://google.serper.dev/search");
            request.Headers.Add("X-API-KEY", ApiKey);
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

            // 1. Проверяем, есть ли прямой готовый ответ Google (Answer Box или Knowledge Graph)
            if (!string.IsNullOrWhiteSpace(result?.AnswerBox?.Answer))
                return result.AnswerBox.Answer;

            if (!string.IsNullOrWhiteSpace(result?.AnswerBox?.Snippet))
                return result.AnswerBox.Snippet;

            // 2. Если прямого ответа нет, собираем результаты обычного поиска (Organic)
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

    // Модели DTO для десериализации ответа Serper.dev
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