using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace suporteEngenhariaUI.Models
{
    public class ContagemConversasApi
    {
        [JsonProperty("new_conversation_count")] // Mapeia o nome do JSON
        public int ContagemNovas { get; set; } // Mantém o nome C# amigável

        [JsonProperty("open_conversation_count")] // Mapeia o nome do JSON
        public int ContagemAbertas { get; set; } // Mantém o nome C# amigável

        [JsonProperty("closed_conversation_count")] // Mapeia o nome do JSON
        public int ContagemEncerradas { get; set; } // Mantém o nome C# amigável
    }
}
