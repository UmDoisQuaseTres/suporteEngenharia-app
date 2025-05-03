using System;
using System.Text.Json.Serialization; // Para [JsonIgnore]

namespace suporteEngenhariaUI.Models // Namespace para modelos
{
    public class ConversationStatusApi
    {
        // Propriedades existentes da API
        public string SenderId { get; set; } = string.Empty;
        public string? ContactName { get; set; }
        public string? Status { get; set; }
        public long CreationTimestamp { get; set; }
        public long? ClosedTimestamp { get; set; }

        // Propriedades Calculadas
        [JsonIgnore] public DateTime CreationDateTime => DateTimeOffset.FromUnixTimeSeconds(CreationTimestamp).LocalDateTime;
        [JsonIgnore] public DateTime? ClosedDateTime => ClosedTimestamp.HasValue ? DateTimeOffset.FromUnixTimeSeconds(ClosedTimestamp.Value).LocalDateTime : null;
        [JsonIgnore] public string DisplayName => !string.IsNullOrWhiteSpace(ContactName) ? ContactName : SenderId;
        [JsonIgnore] public TimeSpan? DuracaoConversa => ClosedDateTime.HasValue ? (ClosedDateTime.Value - CreationDateTime) : (TimeSpan?)null;
    }
}