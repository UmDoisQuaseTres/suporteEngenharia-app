using System.Collections.Generic;
using System.Threading.Tasks;
using suporteEngenhariaUI.Models;   // Referencia os modelos

namespace suporteEngenhariaUI.Interfaces // Namespace para interfaces
{
    public interface IWhatsAppApiService
    {
        // As definições dos métodos GetCountsAsync, GetAllStatusesAsync, CloseConversationAsync
        // permanecem como estavam na resposta anterior, sem implementação.
        // Apenas garanta que os tipos (ContagemConversasApi, etc.) sejam encontrados via 'using'.

        Task<ContagemConversasApi?> GetCountsAsync();
        Task<Dictionary<string, ConversationStatusApi>?> GetAllStatusesAsync();
        Task<CloseStatusApi?> CloseConversationAsync(string senderId);
    }
}