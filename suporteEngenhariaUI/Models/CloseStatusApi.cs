using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace suporteEngenhariaUI.Models
{
    public class CloseStatusApi
    {
        [JsonProperty("status")] // Mapeia o nome do JSON
        public string? Status { get; set; }

        [JsonProperty("error")] // Mapeia o nome do JSON ("error" minúsculo)
        public string? Error { get; set; } // Propriedade para capturar a mensagem de erro da API
    }
}
