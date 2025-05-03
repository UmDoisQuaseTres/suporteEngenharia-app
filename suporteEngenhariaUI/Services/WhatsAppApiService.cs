using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using suporteEngenhariaUI.Exceptions;
using suporteEngenhariaUI.Models;
using suporteEngenhariaUI.Interfaces; // Referencia a interface

namespace suporteEngenhariaUI.Services // Namespace para serviços
{
    public class WhatsAppApiService : IWhatsAppApiService // Implementa a interface
    {
        private readonly HttpClient _httpClient; // HttpClient será injetado

        // Constantes para endpoints
        private const string ApiEndpointContagens = "count";
        private const string ApiEndpointStatuses = "status";
        private const string ApiEndpointClose = "close/{0}";

        // Construtor recebe o HttpClient da DI
        public WhatsAppApiService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            // Configuração (BaseAddress, Timeout, Headers) é feita no Program.cs
        }

        // --- Implementação dos Métodos (Usando seu código original adaptado) ---

        public async Task<ContagemConversasApi?> GetCountsAsync()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(ApiEndpointContagens);
                response.EnsureSuccessStatusCode();
                string jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<ContagemConversasApi>(jsonString, options);
            }
            catch (HttpRequestException httpEx) { Console.WriteLine($"Erro HTTP GetCountsAsync: {httpEx.Message} ({httpEx.StatusCode})"); throw new ApiException($"Erro rede/HTTP contagens: {httpEx.Message}", httpEx); }
            catch (JsonException jsonEx) { Console.WriteLine($"Erro JSON GetCountsAsync: {jsonEx.Message}"); throw new ApiException("Erro processar contagens (JSON?).", jsonEx); }
            catch (Exception ex) { Console.WriteLine($"Erro Inesp. GetCountsAsync: {ex.Message}"); throw new ApiException("Erro inesperado buscar contagens.", ex); }
        }

        public async Task<Dictionary<string, ConversationStatusApi>?> GetAllStatusesAsync()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(ApiEndpointStatuses);
                response.EnsureSuccessStatusCode();
                string jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<Dictionary<string, ConversationStatusApi>>(jsonString, options);
            }
            catch (HttpRequestException httpEx) { Console.WriteLine($"Erro HTTP GetAllStatusesAsync: {httpEx.Message} ({httpEx.StatusCode})"); throw new ApiException($"Erro HTTP {(int?)httpEx.StatusCode ?? 0} status: {httpEx.Message}", httpEx); }
            catch (JsonException jsonEx) { Console.WriteLine($"Erro JSON GetAllStatusesAsync: {jsonEx.Message}"); throw new ApiException("Erro processar status (JSON?).", jsonEx); }
            catch (Exception ex) { Console.WriteLine($"Erro Inesp. GetAllStatusesAsync: {ex.Message}"); throw new ApiException("Erro inesperado buscar status.", ex); }
        }

        public async Task<CloseStatusApi?> CloseConversationAsync(string senderId)
        {
            if (string.IsNullOrEmpty(senderId)) { throw new ArgumentNullException(nameof(senderId)); }

            string endpointFinalizar = string.Format(ApiEndpointClose, senderId);
            HttpResponseMessage? response = null; string responseBody = string.Empty;

            try
            {
                response = await _httpClient.PostAsync(endpointFinalizar, null);
                responseBody = await response.Content.ReadAsStringAsync();
                CloseStatusApi? statusResult = null;
                if (!string.IsNullOrWhiteSpace(responseBody))
                {
                    try
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        statusResult = JsonSerializer.Deserialize<CloseStatusApi>(responseBody, options);
                    }
                    catch (JsonException jsonEx) { Console.WriteLine($"WARN: Erro JSON /close {senderId}: {jsonEx.Message} - Corpo: {responseBody}"); }
                }
                if (statusResult != null) { return statusResult; }
                if (response.IsSuccessStatusCode && statusResult == null) { Console.WriteLine($"WARN: /close {senderId} OK mas sem corpo. Assumindo 'closed'."); return new CloseStatusApi { Status = "closed" }; }
                throw new ApiException($"Falha finalizar. HTTP: {response.StatusCode}.", null, responseBody);
            }
            catch (HttpRequestException httpEx) { Console.WriteLine($"Erro HTTP CloseConvAsync: {httpEx.Message} ({httpEx.StatusCode})"); throw new ApiException($"Erro rede finalizar {senderId}: {httpEx.Message}", httpEx, responseBody); }
            catch (ApiException) { throw; } // Re-lança a ApiException criada acima
            catch (Exception ex) { Console.WriteLine($"Erro Inesp. CloseConvAsync {senderId}: {ex.Message}"); throw new ApiException($"Erro inesperado finalizar {senderId}.", ex, responseBody); }
        }
    }
}