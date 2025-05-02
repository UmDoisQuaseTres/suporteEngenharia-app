using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace suporteEngenhariaUI
{
    public class ContagemConversasApi
    {
        [JsonPropertyName("new_conversation_count")] public int ContagemNovas { get; set; }
        [JsonPropertyName("open_conversation_count")] public int ContagemAbertas { get; set; }
        [JsonPropertyName("closed_conversation_count")] public int ContagemEncerradas { get; set; }
    }
}
