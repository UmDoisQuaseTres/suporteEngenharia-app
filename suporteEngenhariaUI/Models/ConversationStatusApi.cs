using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace suporteEngenhariaUI.Models // Namespace para modelos
{
    public class ConversationStatusApi
    {
        [JsonProperty("sender_id")]
        public string SenderId { get; set; } = string.Empty; // Inicializar para evitar null warnings

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("creation_timestamp")]
        public long CreationTimestampEpoch { get; set; } // Armazena o valor long do JSON

        [JsonProperty("closed_timestamp")]
        public long? ClosedTimestampEpoch { get; set; } // Armazena o valor long? (pode ser null)

        [JsonProperty("contact_name")]
        public string? ContactName { get; set; }

        // --- Propriedades Auxiliares (Calculadas) para facilitar o uso no C# ---

        [JsonIgnore] // Para não tentar desserializar/serializar esta propriedade
        public DateTime CreationTimestamp => DateTimeOffset.FromUnixTimeSeconds(CreationTimestampEpoch).LocalDateTime;

        [JsonIgnore]
        public DateTime? ClosedTimestamp => ClosedTimestampEpoch.HasValue
                                                ? DateTimeOffset.FromUnixTimeSeconds(ClosedTimestampEpoch.Value).LocalDateTime
                                                : (DateTime?)null;

        [JsonIgnore]
        public string DisplayName => string.IsNullOrWhiteSpace(ContactName) ? SenderId : ContactName; // Usa nome se disponível, senão ID

        [JsonIgnore]
        public TimeSpan? DuracaoConversa
        {
            get
            {
                if (Status == "closed" && ClosedTimestamp.HasValue)
                {
                    // Garante que a data de fechamento não seja anterior à de criação
                    if (ClosedTimestamp.Value >= CreationTimestamp)
                    {
                        return ClosedTimestamp.Value - CreationTimestamp;
                    }
                    else
                    {
                        // Log ou tratamento para dados inconsistentes (fechamento antes da criação)
                        Console.WriteLine($"AVISO: Dados inconsistentes para SenderId {SenderId}. ClosedTimestamp ({ClosedTimestamp.Value}) anterior a CreationTimestamp ({CreationTimestamp}).");
                        return TimeSpan.Zero; // Ou null, ou outra indicação de erro
                    }
                }
                return null; // Não está fechada ou não tem timestamp de fechamento
            }
        }
    }
}