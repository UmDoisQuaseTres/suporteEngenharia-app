using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace suporteEngenhariaUI
{
    public class WhatsAppApiService
    {
        private static readonly HttpClient client = new HttpClient();
        private const string ApiBaseUrl = "http://192.168.15.49:5000/"; // <<< VERIFIQUE

        private const string ApiEndpointContagens = "count";
        private const string ApiEndpointStatuses = "status";
        private const string ApiEndpointClose = "close/{0}";

        static WhatsAppApiService()
        {
            try
            {
                client.BaseAddress = new Uri(ApiBaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                // client.Timeout = TimeSpan.FromSeconds(30);
            }
            catch (UriFormatException ex)
            {
                MessageBox.Show($"URL base da API configurada no serviço ('{ApiBaseUrl}') é inválida: {ex.Message}\nA aplicação não poderá se comunicar com a API.",
                                "Erro Crítico de Configuração", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        public async Task<ContagemConversasApi?> GetCountsAsync()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(ApiEndpointContagens);
                response.EnsureSuccessStatusCode(); // Lança HttpRequestException em caso de falha HTTP
                string jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<ContagemConversasApi>(jsonString, options);
            }
            catch (HttpRequestException httpEx)
            {
                // CORREÇÃO: Não tenta acessar httpEx.Response.Content
                Console.WriteLine($"Erro de Rede/HTTP em GetCountsAsync: {httpEx.Message} (StatusCode: {httpEx.StatusCode})");
                throw new ApiException($"Erro de rede/HTTP ao buscar contagens: {httpEx.Message}", httpEx); // Re-lança com a exceção original
            }
            catch (JsonException jsonEx) { /* ... tratamento JsonException ... */ throw new ApiException("Erro ao processar contagens.", jsonEx); }
            catch (Exception ex) { /* ... tratamento Exception ... */ throw new ApiException("Erro inesperado ao buscar contagens.", ex); }
        }

        public async Task<Dictionary<string, ConversationStatusApi>?> GetAllStatusesAsync()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(ApiEndpointStatuses);
                response.EnsureSuccessStatusCode();
                string jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<Dictionary<string, ConversationStatusApi>>(jsonString, options);
            }
            catch (HttpRequestException httpEx)
            {
                // CORREÇÃO: Não tenta acessar httpEx.Response.Content
                Console.WriteLine($"Erro de Rede/HTTP em GetAllStatusesAsync: {httpEx.Message} (StatusCode: {httpEx.StatusCode})");
                // Cria a ApiException sem a resposta, pois pode não estar disponível
                throw new ApiException($"Erro HTTP {httpEx.StatusCode ?? System.Net.HttpStatusCode.BadRequest} ao buscar status: {httpEx.Message}", httpEx);
            }
            catch (JsonException jsonEx) { /* ... tratamento JsonException ... */ throw new ApiException("Erro ao processar resposta da API (JSON inválido?).", jsonEx); }
            catch (Exception ex) { /* ... tratamento Exception ... */ throw new ApiException("Erro inesperado ao buscar status.", ex); }
        }

        public async Task<CloseStatusApi?> CloseConversationAsync(string senderId)
        {
            if (string.IsNullOrEmpty(senderId)) return null;

            string endpointFinalizar = string.Format(ApiEndpointClose, senderId);
            CloseStatusApi? statusResult = null;
            HttpResponseMessage? response = null; // Variável para armazenar a resposta se obtida
            string responseBody = string.Empty;

            try
            {
                response = await client.PostAsync(endpointFinalizar, null); // Armazena a resposta
                responseBody = await response.Content.ReadAsStringAsync(); // Lê o corpo

                if (response.IsSuccessStatusCode || !string.IsNullOrWhiteSpace(responseBody))
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(responseBody)) // Só desserializa se houver corpo
                        {
                            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                            statusResult = JsonSerializer.Deserialize<CloseStatusApi>(responseBody, options);
                        }
                    }
                    catch (JsonException jsonEx) { Console.WriteLine($"Erro ao desserializar resposta de /close (SenderId: {senderId}): {jsonEx.Message} - Corpo: {responseBody}"); if (!response.IsSuccessStatusCode) throw new ApiException($"Falha ao finalizar. Status HTTP: {response.StatusCode}. Resposta não pôde ser processada.", jsonEx, responseBody); }
                }

                // Se HTTP OK, mas não conseguimos desserializar (ou não tinha corpo), assume closed
                if (response.IsSuccessStatusCode && statusResult == null)
                {
                    return new CloseStatusApi { Status = "closed", Error = null };
                }

                // Se HTTP não foi OK E não conseguimos ler um status específico da resposta
                if (!response.IsSuccessStatusCode && statusResult == null)
                {
                    throw new ApiException($"Falha ao finalizar conversa. Status HTTP: {response.StatusCode}.", null, responseBody);
                }

                // Retorna o status lido (pode ser "closed", "already_closed", "not_found", etc.)
                // ou um erro genérico se statusResult ainda for null por algum motivo
                return statusResult ?? new CloseStatusApi { Status = "unknown_error", Error = $"Status HTTP: {response.StatusCode}" };

            }
            catch (HttpRequestException httpEx)
            {
                // CORREÇÃO: Não tenta acessar httpEx.Response.Content
                Console.WriteLine($"Erro de Rede/HTTP em CloseConversationAsync: {httpEx.Message} (StatusCode: {httpEx.StatusCode})");
                // Se tivemos uma resposta parcial antes da exceção, usamos o corpo dela, senão null
                string? errorResponseBody = response != null ? responseBody : null;
                throw new ApiException($"Erro de rede ao finalizar conversa com {senderId}.", httpEx, errorResponseBody);
            }
            catch (Exception ex) { /* ... tratamento Exception ... */ throw new ApiException($"Erro inesperado ao finalizar conversa com {senderId}.", ex); }
        }
    }

    // (Opcional) Classe de Exceção Personalizada para Erros da API
    public class ApiException : Exception
    {
        public string? ApiResponse { get; }

        public ApiException(string message) : base(message) { }
        public ApiException(string message, Exception innerException) : base(message, innerException) { }
        public ApiException(string message, Exception? innerException = null, string? apiResponse = null) : base(message, innerException)
        {
            ApiResponse = apiResponse;
        }
    }
}