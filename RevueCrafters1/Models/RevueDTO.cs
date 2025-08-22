
using System.Text.Json.Serialization;


namespace RevueCrafters1.Models
{
    class RevueDTO
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

    }
}
