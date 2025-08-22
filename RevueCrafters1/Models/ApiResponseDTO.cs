
using System.Text.Json.Serialization;


namespace RevueCrafters1.Models
{
    class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("revueId")]
        public string? RevueId { get; set; }
    }
}
