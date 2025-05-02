using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace suporteEngenhariaUI
{
    public class ConversationStatusApi
    {
        [JsonPropertyName("sender_id")] public required string SenderId { get; set; }
        [JsonPropertyName("status")] public required string Status { get; set; }
        [JsonPropertyName("contact_name")] public string? ContactName { get; set; }
        [JsonPropertyName("creation_timestamp")] public long CreationTimestamp { get; set; }
        [JsonPropertyName("closed_timestamp")] public long? ClosedTimestamp { get; set; }
        [JsonIgnore] public DateTime CreationDateTime => DateTimeOffset.FromUnixTimeSeconds(CreationTimestamp).LocalDateTime;
        [JsonIgnore] public DateTime? ClosedDateTime => ClosedTimestamp.HasValue ? DateTimeOffset.FromUnixTimeSeconds(ClosedTimestamp.Value).LocalDateTime : (DateTime?)null;
    }
}
