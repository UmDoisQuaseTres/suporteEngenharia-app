using suporteEngenhariaUI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace suporteEngenhariaUI.Interfaces
{
    public interface IWhatsAppApiService
    {

        Task<ContagemConversasApi> GetCountsAsync();

        Task<Dictionary<string, ConversationStatusApi>> GetAllStatusesAsync();

        Task<CloseStatusApi> CloseConversationAsync(string senderId);

        Task<ContagemConversasApi> RecalculateCountersAsync();
    }
}