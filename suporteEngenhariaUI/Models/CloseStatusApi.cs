using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace suporteEngenhariaUI.Models
{
    public class CloseStatusApi
    {
        [JsonPropertyName("status")] public required string Status { get; set; }
        [JsonPropertyName("error")] public string? Error { get; set; }
    }
}
