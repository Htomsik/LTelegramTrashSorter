using Newtonsoft.Json;

namespace LTGTrashSorter.Core.Models.DTO;

public class AnalyzerResponseDTO
{
    [JsonProperty("Message")]
    public string Message { get; set; } = string.Empty;
}