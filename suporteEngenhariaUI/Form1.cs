using System.Net.Http.Headers;
using System.Text; // Necessário para StringBuilder
using System.Text.Json;
using System.Text.Json.Serialization;

namespace suporteEngenhariaUI
{
    public partial class Form1 : Form
    {
        // --- HttpClient Configuração ---
        private static readonly HttpClient client = new HttpClient();
        private const string ApiBaseUrl = "http://192.168.15.49:5000/"; // Verifique se este IP ainda é válido

        // --- ENDPOINTS DA API PYTHON ---
        private const string ApiEndpointContagens = "count";
        private const string ApiEndpointStatuses = "status";
        private const string ApiEndpointClose = "close/{0}";

        // --- Construtor --- //
        public Form1()
        {
            InitializeComponent();

            // Configura HttpClient
            try
            {
                client.BaseAddress = new Uri(ApiBaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            catch (UriFormatException ex)
            {
                MessageBox.Show($"URL base da API inválida ('{ApiBaseUrl}'): {ex.Message}", "Erro Crítico", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }
        // --- Fim do Construtor --- //

        // --- Evento Load ---
        private async void Form1_Load(object sender, EventArgs e)
        {
            LimparDetalhes();
            await CarregarDadosIniciaisAsync();
        }

        // Atualização do método de carregamento geral
        private async Task CarregarDadosIniciaisAsync()
        {
            IniciarCarregamento();
            List<Exception> erros = new List<Exception>();

            try
            {
                try
                {
                    await CarregarContagensAsync();
                }
                catch (Exception ex)
                {
                    erros.Add(ex);
                    Console.WriteLine($"Erro ao carregar contagens: {ex.Message}");
                }

                try
                {
                    await CarregarConversasAbertasAsync();
                }
                catch (Exception ex)
                {
                    erros.Add(ex);
                    Console.WriteLine($"Erro ao carregar conversas abertas: {ex.Message}");
                }

                try
                {
                    await CarregarConversasEncerradasAsync();
                }
                catch (Exception ex)
                {
                    erros.Add(ex);
                    Console.WriteLine($"Erro ao carregar conversas encerradas: {ex.Message}");
                }

                // Se algum erro foi detectado, mostra uma mensagem
                if (erros.Count > 0)
                {
                    MostrarErro($"Alguns dados não puderam ser carregados ({erros.Count} erro(s)). A interface pode estar desatualizada.", erros.FirstOrDefault());
                }
            }
            catch (Exception ex)
            {
                MostrarErro("Erro geral ao carregar dados iniciais", ex);
            }
            finally
            {
                FinalizarCarregamento();
            }
        }


        // --- Métodos Auxiliares de Carregamento UI ---
        private void IniciarCarregamento()
        {
            if (this.InvokeRequired) { this.Invoke(new Action(IniciarCarregamento)); return; }
            this.Cursor = Cursors.WaitCursor;
            btnFinalizarSelecionada.Enabled = false;
            btnAtualizarEncerradas.Enabled = false;
            button1.Enabled = false;
            btnAtualizarAbertas.Enabled = false;
            LimparDetalhes(); // Limpa SÓ os detalhes
            toolStripStatusLabelInfo.Text = "Carregando dados...";
            toolStripProgressBar.Visible = true; // Mostra a barra de progresso
        }

        private void FinalizarCarregamento()
        {
            if (this.InvokeRequired) { this.Invoke(new Action(FinalizarCarregamento)); return; }
            this.Cursor = Cursors.Default;
            btnAtualizarEncerradas.Enabled = true;
            button1.Enabled = true;
            btnAtualizarAbertas.Enabled = true;
            // Habilita finalizar APENAS se algo estiver selecionado na lista de ABERTAS
            btnFinalizarSelecionada.Enabled = listViewAbertasParaFinalizar.SelectedItems.Count > 0;
            toolStripStatusLabelInfo.Text = $"Pronto. Dados atualizados às {DateTime.Now:HH:mm:ss}";
            toolStripProgressBar.Visible = false; // Esconde a barra de progresso
        }

        // --- Busca Dados API: Contagens ---
        private async Task CarregarContagensAsync()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(ApiEndpointContagens);
                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var contagens = JsonSerializer.Deserialize<ContagemConversasApi>(jsonString, options);
                    if (contagens != null) { AtualizarLabelsContagem(contagens.ContagemNovas, contagens.ContagemAbertas, contagens.ContagemEncerradas); }
                }
                else { Console.WriteLine($"Erro HTTP {response.StatusCode} ao buscar contagens."); AtualizarLabelsContagem(-1, -1, -1); }
            }
            catch (Exception ex) { Console.WriteLine($"Erro na requisição de contagens: {ex.Message}"); AtualizarLabelsContagem(-1, -1, -1); throw; }
        }

        // --- Busca Dados API: Conversas (Base) ---
        private async Task<Dictionary<string, ConversationStatusApi>?> BuscarStatusConversoesAsync()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(ApiEndpointStatuses);
                if (!response.IsSuccessStatusCode) { MostrarErro($"Erro HTTP {response.StatusCode} ao buscar status", null, await response.Content.ReadAsStringAsync()); return null; }
                string jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<Dictionary<string, ConversationStatusApi>>(jsonString, options);
            }
            catch (JsonException jsonEx) { MostrarErro("Erro ao desserializar dados de status (JSON inválido?)", jsonEx); return null; }
            catch (HttpRequestException httpEx) { MostrarErro("Erro de rede ao buscar status", httpEx); return null; }
            catch (Exception ex) { MostrarErro("Erro inesperado ao buscar status", ex); throw; }
        }

        private async Task CarregarConversasAbertasAsync()
        {
            try
            {
                var todosStatus = await BuscarStatusConversoesAsync();
                if (todosStatus != null)
                {
                    var conversasAbertas = todosStatus.Values
                        .Where(conv => conv.Status != null && conv.Status.Equals("open", StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(conv => conv.CreationTimestamp)
                        .ToList();

                    // Preenche a lista, mesmo se estiver vazia (mas os dados vieram da API com sucesso)
                    PopularListViewAbertas(conversasAbertas);
                }
                // Se todosStatus for null, não faz nada para preservar os dados já exibidos
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro capturado em CarregarConversasAbertasAsync: {ex.Message}");
                throw;
            }
        }


        // --- Busca Dados API: Conversas Encerradas ---
        private async Task CarregarConversasEncerradasAsync()
        {
            try
            {
                var todosStatus = await BuscarStatusConversoesAsync();
                if (todosStatus != null)
                {
                    var conversasEncerradas = todosStatus.Values
                        .Where(conv => conv.Status != null && conv.Status.Equals("closed", StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(conv => conv.ClosedTimestamp ?? conv.CreationTimestamp)
                        .ToList();

                    // Preenche a lista, mesmo se estiver vazia (mas os dados vieram da API com sucesso)
                    PopularListViewEncerradas(conversasEncerradas);
                }
                // Se todosStatus for null, não faz nada para preservar os dados já exibidos
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro capturado em CarregarConversasEncerradasAsync: {ex.Message}");
                throw;
            }
        }



        // --- Ação API: Finalizar Conversa ---
        private async Task FinalizarConversaApiAsync(string senderId)
        {
            if (string.IsNullOrEmpty(senderId)) return;
            IniciarCarregamento();
            try
            {
                string endpointFinalizar = string.Format(ApiEndpointClose, senderId);
                HttpResponseMessage response = await client.PostAsync(endpointFinalizar, null);
                string responseBody = await response.Content.ReadAsStringAsync();
                CloseStatusApi? statusResult = null;
                if (!string.IsNullOrWhiteSpace(responseBody))
                {
                    try
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        // *** Usa a DTO CloseStatusApi (com Error nullable) ***
                        statusResult = JsonSerializer.Deserialize<CloseStatusApi>(responseBody, options);
                    }
                    catch (JsonException jsonEx) { Console.WriteLine($"Erro ao desserializar resposta de /close: {jsonEx.Message}"); }
                }

                // --- LÓGICA DE VERIFICAÇÃO ---
                if (response.IsSuccessStatusCode && statusResult?.Status == "closed") // Verifica SUCESSO primeiro
                {
                    MessageBox.Show($"Conversa com {senderId} finalizada com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RemoverItemListViewAbertas(senderId);
                    LimparDetalhes();
                    // Recarrega contagens e encerradas em background
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(100);
                        await CarregarContagensAsync();
                        await CarregarConversasEncerradasAsync();
                    });
                }
                // Trata casos específicos DEPOIS do sucesso principal
                else if (statusResult?.Status == "already_closed")
                {
                    MessageBox.Show($"Conversa com {senderId} já estava finalizada.", "Informação", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RemoverItemListViewAbertas(senderId);
                    LimparDetalhes();
                    _ = Task.Run(async () => { /* ... recarrega ... */ });
                }
                else if (statusResult?.Status == "not_found" || response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    MessageBox.Show($"Conversa com {senderId} não encontrada na API.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    RemoverItemListViewAbertas(senderId);
                    LimparDetalhes();
                    _ = Task.Run(async () => { /* ... recarrega ... */ });
                }
                else // Se não for sucesso OU o status JSON não for um dos esperados
                {
                    MostrarErro($"Falha ao finalizar conversa com {senderId}. Status HTTP: {response.StatusCode}", null, responseBody);
                }
            }
            catch (HttpRequestException httpEx) { MostrarErro($"Erro de rede ao finalizar conversa com {senderId}", httpEx); }
            catch (Exception ex) { MostrarErro($"Erro inesperado ao finalizar conversa com {senderId}", ex); }
            finally
            {
                FinalizarCarregamento();
            }
        }

        // --- Métodos de Atualização da UI ---

        private void AtualizarLabelsContagem(int novas, int abertas, int encerradas)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => AtualizarLabelsContagem(novas, abertas, encerradas))); return; }
            lblContagemNovas.Text = novas < 0 ? "Novas: Erro" : $"Novas: {novas}";
            lblContagemAbertas.Text = abertas < 0 ? "Em Aberto: Erro" : $"Em Aberto: {abertas}";
            lblContagemEncerradas.Text = encerradas < 0 ? "Encerradas: Erro" : $"Encerradas: {encerradas}";
        }

        // Popula ListView de Abertas
        private void PopularListViewAbertas(List<ConversationStatusApi> conversas)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => PopularListViewAbertas(conversas))); return; }

            // Guarda o item selecionado para restaurá-lo depois
            string? selectedSenderId = null;
            if (listViewAbertasParaFinalizar.SelectedItems.Count > 0 &&
                listViewAbertasParaFinalizar.SelectedItems[0].Tag is ConversationStatusApi sc1)
                selectedSenderId = sc1.SenderId;

            // Só atualiza se recebemos uma lista válida da API (mesmo que vazia)
            if (conversas != null)
            {
                listViewAbertasParaFinalizar.BeginUpdate();
                listViewAbertasParaFinalizar.Items.Clear();

                DateTime agora = DateTime.Now;
                foreach (var conv in conversas)
                {
                    string di = !string.IsNullOrWhiteSpace(conv.ContactName) ? conv.ContactName : conv.SenderId;
                    ListViewItem item = new ListViewItem(di);
                    item.SubItems.Add(conv.CreationDateTime.ToString("dd/MM/yy HH:mm:ss"));
                    item.SubItems.Add(conv.Status ?? "");
                    TimeSpan da = agora - conv.CreationDateTime;
                    item.SubItems.Add(FormatarTempoDecorrido(da));
                    item.Tag = conv;
                    listViewAbertasParaFinalizar.Items.Add(item);

                    if (conv.SenderId == selectedSenderId)
                    {
                        item.Selected = true;
                        item.Focused = true;
                    }
                }
                listViewAbertasParaFinalizar.EndUpdate();

                if (listViewAbertasParaFinalizar.SelectedItems.Count == 0)
                    LimparDetalhes();
                else if (selectedSenderId != null && listViewAbertasParaFinalizar.SelectedItems.Count > 0)
                {
                    MostrarDetalhesConversaSelecionada((ConversationStatusApi)listViewAbertasParaFinalizar.SelectedItems[0].Tag);
                    btnFinalizarSelecionada.Enabled = true;
                }
            }
        }


        // O mesmo padrão para a lista de encerradas
        private void PopularListViewEncerradas(List<ConversationStatusApi> conversas)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => PopularListViewEncerradas(conversas))); return; }

            // Guarda o item selecionado para restaurá-lo depois
            string? selectedSenderId = null;
            if (listViewEncerradas.SelectedItems.Count > 0 &&
                listViewEncerradas.SelectedItems[0].Tag is ConversationStatusApi sc2)
                selectedSenderId = sc2.SenderId;

            // Só atualiza se recebemos uma lista válida da API (mesmo que vazia)
            if (conversas != null)
            {
                listViewEncerradas.BeginUpdate();
                listViewEncerradas.Items.Clear();

                foreach (var conv in conversas)
                {
                    string di = !string.IsNullOrWhiteSpace(conv.ContactName) ? conv.ContactName : conv.SenderId;
                    ListViewItem item = new ListViewItem(di);
                    item.SubItems.Add(conv.CreationDateTime.ToString("dd/MM/yy HH:mm:ss"));
                    item.SubItems.Add(conv.Status ?? "");
                    item.SubItems.Add(conv.ClosedDateTime?.ToString("dd/MM/yy HH:mm:ss") ?? "N/A");
                    TimeSpan dt = conv.ClosedDateTime.HasValue ? (conv.ClosedDateTime.Value - conv.CreationDateTime) : TimeSpan.Zero;
                    item.SubItems.Add(FormatarTempoDecorrido(dt));
                    item.Tag = conv;
                    listViewEncerradas.Items.Add(item);

                    if (conv.SenderId == selectedSenderId)
                    {
                        item.Selected = true;
                        item.Focused = true;
                    }
                }
                listViewEncerradas.EndUpdate();

                if (listViewEncerradas.SelectedItems.Count == 0 && listViewAbertasParaFinalizar.SelectedItems.Count == 0)
                    LimparDetalhes();
                else if (selectedSenderId != null && listViewEncerradas.SelectedItems.Count > 0)
                {
                    MostrarDetalhesConversaSelecionada((ConversationStatusApi)listViewEncerradas.SelectedItems[0].Tag);
                    btnFinalizarSelecionada.Enabled = false;
                }
            }
        }

        // Função auxiliar para formatar TimeSpan
        private string FormatarTempoDecorrido(TimeSpan duracao)
        {
            if (duracao.TotalSeconds <= 0) return "< 1m"; var sb = new StringBuilder(); if (duracao.Days > 0) sb.Append($"{duracao.Days}d "); if (duracao.Days > 0 || duracao.Hours > 0) if (duracao.Hours > 0) sb.Append($"{duracao.Hours}h "); if (duracao.TotalHours > 0 || duracao.Minutes > 0) if (duracao.Minutes > 0) sb.Append($"{duracao.Minutes}m"); if (sb.Length == 0) return "< 1m"; return sb.ToString().Trim();
        }

        // Limpa os detalhes
        private void LimparDetalhes()
        {
            if (this.InvokeRequired) { this.Invoke(new Action(LimparDetalhes)); return; }
            lblValorSenderId.Text = string.Empty; lblValorStatus.Text = string.Empty; lblValorLastUpdate.Text = string.Empty; lblValorTempoAberto.Text = string.Empty; btnFinalizarSelecionada.Enabled = false;
        }

        // Mostra detalhes da conversa selecionada
        private void MostrarDetalhesConversaSelecionada(ConversationStatusApi conv)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => MostrarDetalhesConversaSelecionada(conv))); return; }
            if (conv == null) { LimparDetalhes(); return; }
            lblValorSenderId.Text = !string.IsNullOrWhiteSpace(conv.ContactName) ? $"{conv.ContactName} ({conv.SenderId})" : conv.SenderId; lblValorStatus.Text = conv.Status; lblValorLastUpdate.Text = conv.CreationDateTime.ToString("dd/MM/yyyy HH:mm:ss");
            TimeSpan duracao; string tempoAbertoFormatado; if (conv.Status != null && conv.Status.Equals("open", StringComparison.OrdinalIgnoreCase)) { duracao = DateTime.Now - conv.CreationDateTime; tempoAbertoFormatado = FormatarTempoDecorrido(duracao); } else if (conv.ClosedDateTime.HasValue) { duracao = conv.ClosedDateTime.Value - conv.CreationDateTime; tempoAbertoFormatado = FormatarTempoDecorrido(duracao); } else { tempoAbertoFormatado = "N/A"; }
            lblValorTempoAberto.Text = tempoAbertoFormatado;
        }

        // Remove item da lista de Abertas pelo SenderId
        private void RemoverItemListViewAbertas(string senderId)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => RemoverItemListViewAbertas(senderId))); return; }
            for (int i = listViewAbertasParaFinalizar.Items.Count - 1; i >= 0; i--) { ListViewItem item = listViewAbertasParaFinalizar.Items[i]; if (item.Tag is ConversationStatusApi c && c.SenderId == senderId) { listViewAbertasParaFinalizar.Items.RemoveAt(i); return; } }
        }

        // --- Tratamento de Erros (Helper) ---
        private void MostrarErro(string titulo, Exception? ex = null, string? apiMensagem = null)
        {
            string mensagemCompleta = titulo; if (!string.IsNullOrWhiteSpace(apiMensagem)) { string erroApiDetalhe = apiMensagem; try { using (JsonDocument document = JsonDocument.Parse(apiMensagem)) { if (document.RootElement.TryGetProperty("error", out JsonElement errorElement)) { erroApiDetalhe = errorElement.GetString() ?? apiMensagem; } else if (document.RootElement.TryGetProperty("message", out JsonElement msgElement)) { erroApiDetalhe = msgElement.GetString() ?? apiMensagem; } } } catch { /* Ignora */ } mensagemCompleta += $"\n\nAPI: {erroApiDetalhe}"; }
            if (ex != null) { var innerEx = ex; while (innerEx.InnerException != null) innerEx = innerEx.InnerException; mensagemCompleta += $"\n\nTécnico ({innerEx.GetType().Name}): {innerEx.Message}"; }
            // Exibe o título do erro no StatusStrip
            // (Usa Invoke para garantir que seja seguro chamar de qualquer thread)
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => toolStripStatusLabelInfo.Text = $"Erro: {titulo}"));
            }
            else
            {
                toolStripStatusLabelInfo.Text = $"Erro: {titulo}";
            }
            if (this.InvokeRequired) { this.Invoke(new Action(() => MessageBox.Show(this, mensagemCompleta, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error))); } else { MessageBox.Show(this, mensagemCompleta, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            Console.WriteLine($"ERRO: {mensagemCompleta}");
        }


        // --- Event Handlers ---

        // Evento de seleção na lista de Abertas
        private void listViewAbertasParaFinalizar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewAbertasParaFinalizar.SelectedItems.Count > 0) { ListViewItem selectedItem = listViewAbertasParaFinalizar.SelectedItems[0]; if (selectedItem.Tag is ConversationStatusApi conv) { MostrarDetalhesConversaSelecionada(conv); btnFinalizarSelecionada.Enabled = true; } else { LimparDetalhes(); Console.WriteLine("WARN: Tag inválido na seleção (Abertas)."); } } else { LimparDetalhes(); }
        }

        // Evento de seleção na lista de Encerradas
        private void listViewEncerradas_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewEncerradas.SelectedItems.Count > 0) { ListViewItem selectedItem = listViewEncerradas.SelectedItems[0]; if (selectedItem.Tag is ConversationStatusApi conv) { MostrarDetalhesConversaSelecionada(conv); /* Não habilita finalizar */ } else { LimparDetalhes(); Console.WriteLine("WARN: Tag inválido na seleção (Encerradas)."); } } else { LimparDetalhes(); }
        }

        // Evento de clique no botão Finalizar
        private async void btnFinalizarSelecionada_Click(object sender, EventArgs e)
        {
            if (listViewAbertasParaFinalizar.SelectedItems.Count > 0) { ListViewItem selectedItem = listViewAbertasParaFinalizar.SelectedItems[0]; if (selectedItem.Tag is ConversationStatusApi convParaFinalizar) { DialogResult confirmacao = MessageBox.Show($"Finalizar conversa com {(!string.IsNullOrWhiteSpace(convParaFinalizar.ContactName) ? convParaFinalizar.ContactName : convParaFinalizar.SenderId)}?", "Confirmar Finalização", MessageBoxButtons.YesNo, MessageBoxIcon.Question); if (confirmacao == DialogResult.Yes) { await FinalizarConversaApiAsync(convParaFinalizar.SenderId); } } else { MessageBox.Show("Erro ao obter dados da conversa selecionada.", "Erro Interno", MessageBoxButtons.OK, MessageBoxIcon.Warning); } } else { MessageBox.Show("Nenhuma conversa selecionada para finalizar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        }

        // Também atualiza o método do botão da visão geral
        private async void button1_Click(object sender, EventArgs e)
        {
            IniciarCarregamento();
            List<Exception> erros = new List<Exception>();

            try
            {
                try
                {
                    await CarregarContagensAsync();
                }
                catch (Exception ex)
                {
                    erros.Add(ex);
                    Console.WriteLine($"Erro ao carregar contagens: {ex.Message}");
                }

                try
                {
                    await CarregarConversasAbertasAsync();
                }
                catch (Exception ex)
                {
                    erros.Add(ex);
                    Console.WriteLine($"Erro ao carregar conversas abertas: {ex.Message}");
                }

                try
                {
                    await CarregarConversasEncerradasAsync();
                }
                catch (Exception ex)
                {
                    erros.Add(ex);
                    Console.WriteLine($"Erro ao carregar conversas encerradas: {ex.Message}");
                }

                // Se algum erro foi detectado, mostra uma mensagem
                if (erros.Count > 0)
                {
                    MostrarErro($"Alguns dados não puderam ser carregados ({erros.Count} erro(s)). A interface pode estar desatualizada.", erros.FirstOrDefault());
                }
            }
            catch (Exception ex)
            {
                MostrarErro("Erro ao atualizar todos os dados", ex);
            }
            finally
            {
                FinalizarCarregamento();
            }
        }
        // Evento de clique botão Atualizar Encerradas
        private async void btnAtualizarEncerradas_Click(object sender, EventArgs e)
        {
            IniciarCarregamento(); try { await CarregarConversasEncerradasAsync(); } catch (Exception ex) { MostrarErro("Erro ao atualizar lista de conversas encerradas", ex); } finally { FinalizarCarregamento(); }
        }

        // Evento de clique botão Atualizar Abertas
        private async void btnAtualizarAbertas_Click(object sender, EventArgs e)
        {
            IniciarCarregamento(); try { await CarregarConversasAbertasAsync(); } catch (Exception ex) { MostrarErro("Erro ao atualizar lista de conversas abertas", ex); } finally { FinalizarCarregamento(); }
        }

        // Método para liberar recursos ao fechar o formulário
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            client.Dispose(); // Libera os recursos do HttpClient  
        }

    } // Fim da classe Form1

    // ------ Classes DTO ------
    public class ContagemConversasApi
    {
        [JsonPropertyName("new_conversation_count")] public int ContagemNovas { get; set; }
        [JsonPropertyName("open_conversation_count")] public int ContagemAbertas { get; set; }
        [JsonPropertyName("closed_conversation_count")] public int ContagemEncerradas { get; set; }
    }
    public class ConversationStatusApi
    {
        [JsonPropertyName("sender_id")] public required string SenderId { get; set; }
        [JsonPropertyName("status")] public required string Status { get; set; }
        [JsonPropertyName("contact_name")] public string? ContactName { get; set; }
        [JsonPropertyName("creation_timestamp")] public long CreationTimestamp { get; set; }
        [JsonPropertyName("closed_timestamp")] public long? ClosedTimestamp { get; set; }
        [JsonIgnore] public DateTime CreationDateTime => DateTimeOffset.FromUnixTimeSeconds(CreationTimestamp).LocalDateTime;
        [JsonIgnore] public DateTime? ClosedDateTime => ClosedTimestamp.HasValue ? DateTimeOffset.FromUnixTimeSeconds(ClosedTimestamp.Value).LocalDateTime : (DateTime?)null;
    }
    public class CloseStatusApi
    {
        [JsonPropertyName("status")] public required string Status { get; set; } 
        [JsonPropertyName("error")] public string? Error { get; set; } 
    }

} // Fim do namespace