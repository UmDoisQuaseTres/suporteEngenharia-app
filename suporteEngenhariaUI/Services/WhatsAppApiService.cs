using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using suporteEngenhariaUI.Exceptions;
using suporteEngenhariaUI.Interfaces;
using suporteEngenhariaUI.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace suporteEngenhariaUI.Services
{
    public class WhatsAppApiService : IWhatsAppApiService
    {
        private readonly string _apiBaseUrl;
        private readonly HttpClient _httpClient;

        // Definindo os endpoints conforme a API Flask
        private const string EndpointCount = "count";
        private const string EndpointStatus = "status";
        private const string EndpointClose = "close";
        private const string EndpointRecalculateCounters = "recalculate-counters";

        public WhatsAppApiService(IConfiguration configuration)
        {
            _apiBaseUrl = configuration["ApiSettings:WhatsAppApi:BaseUrl"];
            if (string.IsNullOrEmpty(_apiBaseUrl))
            {
                throw new ArgumentException("A URL base da API não está configurada em appsettings.json");
            }

            _httpClient = new HttpClient();
            // Garantir que a URL base termine com "/"
            if (!_apiBaseUrl.EndsWith("/"))
            {
                _apiBaseUrl += "/";
            }
        }

        public async Task<ContagemConversasApi> GetCountsAsync()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{_apiBaseUrl}{EndpointCount}");
                response.EnsureSuccessStatusCode();
                string jsonString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ContagemConversasApi>(jsonString);
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException($"Erro ao obter contagem de conversas: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new ApiException($"Erro inesperado ao obter contagem de conversas: {ex.Message}", ex);
            }
        }

        public async Task<Dictionary<string, ConversationStatusApi>> GetAllStatusesAsync()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{_apiBaseUrl}{EndpointStatus}");
                response.EnsureSuccessStatusCode();
                string jsonString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Dictionary<string, ConversationStatusApi>>(jsonString);
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException($"Erro ao obter status das conversas: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new ApiException($"Erro inesperado ao obter status das conversas: {ex.Message}", ex);
            }
        }

        public async Task<CloseStatusApi> CloseConversationAsync(string senderId)
        {
            if (string.IsNullOrEmpty(senderId))
            {
                throw new ArgumentException("O ID do remetente não pode ser nulo ou vazio");
            }

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(
                    $"{_apiBaseUrl}{EndpointClose}/{senderId}",
                    new StringContent("", Encoding.UTF8, "application/json"));

                response.EnsureSuccessStatusCode();
                string jsonString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CloseStatusApi>(jsonString);
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException($"Erro ao fechar conversa {senderId}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new ApiException($"Erro inesperado ao fechar conversa {senderId}: {ex.Message}", ex);
            }
        }

        public async Task<ContagemConversasApi> RecalculateCountersAsync()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(
                    $"{_apiBaseUrl}{EndpointRecalculateCounters}",
                    new StringContent("", Encoding.UTF8, "application/json"));

                response.EnsureSuccessStatusCode();
                string jsonString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ContagemConversasApi>(jsonString);
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException($"Erro ao recalcular contadores: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new ApiException($"Erro inesperado ao recalcular contadores: {ex.Message}", ex);
            }
        }
    }
}