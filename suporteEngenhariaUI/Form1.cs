
using System.Net.Http.Headers;
using System.Text; // Necessário para StringBuilder
using System.Text.Json;


namespace suporteEngenhariaUI
{
    public partial class Form1 : Form
    {
        // --- HttpClient Configuração ---
        private static readonly HttpClient client = new HttpClient();
        private const string ApiBaseUrl = "http://127.0.0.1:5000/";

        // --- ENDPOINTS DA API PYTHON ---
        private const string ApiEndpointContagens = "count"; // GET
        private const string ApiEndpointStatuses = "status"; // GET
        private const string ApiEndpointClose = "close/{0}"; // POST (com sender_id)

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

        // --- Carregamento Principal ---
        private async Task CarregarDadosIniciaisAsync()
        {
            IniciarCarregamento(); // Centraliza a lógica de início
            try
            {
                await Task.WhenAll(
                    CarregarContagensAsync(),
                    CarregarConversasAbertasAsync(),
                    CarregarConversasEncerradasAsync()
                );
            }
            catch (Exception ex)
            {
                // MostrarErro já lida com a UI thread se necessário
                MostrarErro("Erro geral ao carregar dados iniciais", ex);
            }
            finally
            {
                FinalizarCarregamento(); // Centraliza a lógica de fim
            }
        }

        // --- Métodos Auxiliares de Carregamento UI ---

        //Inicia o carregamento de dados

        private void IniciarCarregamento()
        {
            //Verifica qual thread esta rodando a UI
            if (this.InvokeRequired) { this.Invoke(new Action(IniciarCarregamento)); return; }

            //Cursor de carregamento
            this.Cursor = Cursors.WaitCursor;

            // Desabilita botões de ação/atualização
            btnFinalizarSelecionada.Enabled = false;
            btnAtualizarEncerradas.Enabled = false;
            button1.Enabled = false;

            // Limpa painel de detalhes
            LimparDetalhes();
        }

        //Finaliza carregamento de dados
        private void FinalizarCarregamento()
        {
            //Verifica qual thread esta rodando a UI
            if (this.InvokeRequired) { this.Invoke(new Action(FinalizarCarregamento)); return; }

            this.Cursor = Cursors.Default;
            // Reabilita botões de atualização
            btnAtualizarEncerradas.Enabled = true;
            button1.Enabled = true;
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
                    if (contagens != null)
                    {
                        AtualizarLabelsContagem(contagens.ContagemNovas, contagens.ContagemAbertas, contagens.ContagemEncerradas);
                    }
                }
                else
                {
                    Console.WriteLine($"Erro HTTP {response.StatusCode} ao buscar contagens.");
                    AtualizarLabelsContagem(-1, -1, -1); // Indica erro na UI
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na requisição de contagens: {ex.Message}");
                AtualizarLabelsContagem(-1, -1, -1); // Indica erro na UI
                throw; // Re-lança para ser pego pelo CarregarDadosIniciaisAsync
            }
        }

        // --- Busca Dados API: Conversas (Base) ---
        private async Task<Dictionary<string, ConversationStatusApi>?> BuscarStatusConversoesAsync()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(ApiEndpointStatuses);

                if (!response.IsSuccessStatusCode)
                {
                    // Usa o helper MostrarErro que já trata UI thread
                    MostrarErro($"Erro HTTP {response.StatusCode} ao buscar status", null, await response.Content.ReadAsStringAsync());
                    return null;
                }

                string jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<Dictionary<string, ConversationStatusApi>>(jsonString, options);
            }
            catch (JsonException jsonEx) // Erro específico de desserialização
            {
                MostrarErro("Erro ao desserializar dados de status (JSON inválido?)", jsonEx);
                return null;
            }
            catch (HttpRequestException httpEx) // Erro específico de rede
            {
                MostrarErro("Erro de rede ao buscar status", httpEx);
                return null;
            }
            catch (Exception ex) // Outros erros
            {
                MostrarErro("Erro inesperado ao buscar status", ex);
                throw; // Re-lança para indicar falha crítica na operação pai
            }
        }

        // --- Busca Dados API: Conversas Abertas ---
        private async Task CarregarConversasAbertasAsync()
        {
            try
            {
                var todosStatus = await BuscarStatusConversoesAsync();
                if (todosStatus != null)
                {
                    List<ConversationStatusApi> conversasAbertas = todosStatus.Values
                        .Where(conv => conv.Status != null && conv.Status.Equals("open", StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(conv => conv.CreationTimestamp)
                        .ToList();
                    PopularListViewAbertas(conversasAbertas); // Popula a UI
                }
                else
                {
                    PopularListViewAbertas(new List<ConversationStatusApi>()); // Limpa a lista se a busca falhou
                }
            }
            catch (Exception ex)
            {
                // O erro já foi mostrado por BuscarStatusConversoesAsync ou será pego por CarregarDadosIniciaisAsync
                Console.WriteLine($"Erro capturado em CarregarConversasAbertasAsync: {ex.Message}");
                PopularListViewAbertas(new List<ConversationStatusApi>()); // Garante que a lista seja limpa
                throw; // Re-lança para CarregarDadosIniciaisAsync
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
                    List<ConversationStatusApi> conversasEncerradas = todosStatus.Values
                        .Where(conv => conv.Status != null && conv.Status.Equals("closed", StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(conv => conv.ClosedTimestamp ?? conv.CreationTimestamp) // Ordena por fechamento ou criação
                        .ToList();
                    PopularListViewEncerradas(conversasEncerradas); // Popula a UI
                }
                else
                {
                    PopularListViewEncerradas(new List<ConversationStatusApi>()); // Limpa a lista se a busca falhou
                }
            }
            catch (Exception ex)
            {
                // O erro já foi mostrado por BuscarStatusConversoesAsync ou será pego por CarregarDadosIniciaisAsync
                Console.WriteLine($"Erro capturado em CarregarConversasEncerradasAsync: {ex.Message}");
                PopularListViewEncerradas(new List<ConversationStatusApi>()); // Garante que a lista seja limpa
                throw; // Re-lança para CarregarDadosIniciaisAsync
            }
        }

        // --- Ação API: Finalizar Conversa ---
        private async Task FinalizarConversaApiAsync(string senderId)
        {
            if (string.IsNullOrEmpty(senderId)) return;

            IniciarCarregamento(); // Mostra feedback visual de ação

            try
            {
                string endpointFinalizar = string.Format(ApiEndpointClose, senderId);
                HttpResponseMessage response = await client.PostAsync(endpointFinalizar, null);
                string responseBody = await response.Content.ReadAsStringAsync(); // Lê o corpo SEMPRE

                CloseStatusApi? statusResult = null;
                if (!string.IsNullOrWhiteSpace(responseBody)) // Tenta desserializar apenas se houver corpo
                {
                    try
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        statusResult = JsonSerializer.Deserialize<CloseStatusApi>(responseBody, options);
                    }
                    catch (JsonException jsonEx)
                    {
                        Console.WriteLine($"Erro ao desserializar resposta de /close: {jsonEx.Message}");
                        // Continua, pois o status HTTP pode ser suficiente
                    }
                }

                // Verifica o resultado da operação
                if (response.IsSuccessStatusCode && statusResult?.Status == "closed")
                {
                    MessageBox.Show($"Conversa com {senderId} finalizada com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Atualiza UI localmente primeiro para resposta rápida
                    RemoverItemListViewAbertas(senderId);
                    LimparDetalhes();
                    // Recarrega dados em segundo plano para garantir consistência
                    _ = Task.Run(async () =>
                    { // Roda em background para não bloquear UI
                        await Task.Delay(100); // Pequeno delay opcional
                        await CarregarContagensAsync();
                        await CarregarConversasEncerradasAsync();
                    });
                }
                else if (statusResult?.Status == "already_closed")
                {
                    MessageBox.Show($"Conversa com {senderId} já estava finalizada.", "Informação", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RemoverItemListViewAbertas(senderId);
                    LimparDetalhes();
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(100);
                        await CarregarContagensAsync();
                        await CarregarConversasEncerradasAsync();
                    });
                }
                else if (statusResult?.Status == "not_found" || response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    MessageBox.Show($"Conversa com {senderId} não encontrada na API.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    RemoverItemListViewAbertas(senderId);
                    LimparDetalhes();
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(100);
                        await CarregarContagensAsync();
                        await CarregarConversasEncerradasAsync();
                    });
                }
                else
                {
                    MostrarErro($"Falha ao finalizar conversa com {senderId}. Status HTTP: {response.StatusCode}", null, responseBody);
                }
            }
            catch (HttpRequestException httpEx) // Erro específico de rede
            {
                MostrarErro($"Erro de rede ao finalizar conversa com {senderId}", httpEx);
            }
            catch (Exception ex) // Outros erros inesperados
            {
                MostrarErro($"Erro inesperado ao finalizar conversa com {senderId}", ex);
            }
            finally
            {
                FinalizarCarregamento();
                // Restaura botão de finalizar APENAS se uma linha AINDA estiver selecionada na lista de abertas
                btnFinalizarSelecionada.Enabled = listViewAbertasParaFinalizar.SelectedItems.Count > 0;
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

            listViewAbertasParaFinalizar.BeginUpdate();
            listViewAbertasParaFinalizar.Items.Clear(); // Limpa sempre antes de popular

            if (conversas != null)
            {
                DateTime agora = DateTime.Now;
                foreach (var conv in conversas)
                {
                    // Coluna 1: Nome ou ID
                    string displayIdentifier = !string.IsNullOrWhiteSpace(conv.ContactName) ? conv.ContactName : conv.SenderId;
                    ListViewItem item = new ListViewItem(displayIdentifier);
                    // Coluna 2: Data Criação
                    item.SubItems.Add(conv.CreationDateTime.ToString("dd/MM/yy HH:mm:ss"));
                    // Coluna 3: Status
                    item.SubItems.Add(conv.Status ?? "");
                    // Coluna 4: Tempo Aberto (Calculado)
                    TimeSpan duracaoAberta = agora - conv.CreationDateTime;
                    item.SubItems.Add(FormatarTempoDecorrido(duracaoAberta));

                    item.Tag = conv; // Armazena DTO completo
                    listViewAbertasParaFinalizar.Items.Add(item);
                }
            }

            // Ajusta colunas (Tente ColumnContent primeiro)
            try { listViewAbertasParaFinalizar.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent); }
            catch { listViewAbertasParaFinalizar.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize); } // Fallback

            // Ajuste fino opcional se necessário (exemplo: garantir largura mínima)
            // foreach(ColumnHeader col in listViewAbertasParaFinalizar.Columns) {
            //     if (col.Width < 80) col.Width = 80;
            // }

            listViewAbertasParaFinalizar.EndUpdate();
        }

        // Popula ListView de Encerradas
        private void PopularListViewEncerradas(List<ConversationStatusApi> conversas)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => PopularListViewEncerradas(conversas))); return; }

            listViewEncerradas.BeginUpdate();
            listViewEncerradas.Items.Clear(); // Limpa sempre

            if (conversas != null)
            {
                foreach (var conv in conversas)
                {
                    // Coluna 1: Nome ou ID
                    string displayIdentifier = !string.IsNullOrWhiteSpace(conv.ContactName) ? conv.ContactName : conv.SenderId;
                    ListViewItem item = new ListViewItem(displayIdentifier);
                    // Coluna 2: Data Criação
                    item.SubItems.Add(conv.CreationDateTime.ToString("dd/MM/yy HH:mm:ss"));
                    // Coluna 3: Status ("closed")
                    item.SubItems.Add(conv.Status ?? "");
                    // Coluna 4: Data Fechamento
                    item.SubItems.Add(conv.ClosedDateTime?.ToString("dd/MM/yy HH:mm:ss") ?? "N/A");
                    // Coluna 5: Tempo Aberto (Calculado)
                    TimeSpan duracaoTotal = conv.ClosedDateTime.HasValue
                                            ? (conv.ClosedDateTime.Value - conv.CreationDateTime)
                                            : TimeSpan.Zero;
                    item.SubItems.Add(FormatarTempoDecorrido(duracaoTotal));

                    item.Tag = conv; // Armazena DTO completo
                    listViewEncerradas.Items.Add(item);
                }
            }

            // Ajusta colunas (Tente ColumnContent primeiro)
            try { listViewEncerradas.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent); }
            catch { listViewEncerradas.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize); } // Fallback

            // Ajuste fino opcional se necessário

            listViewEncerradas.EndUpdate();
        }

        // Função auxiliar para formatar TimeSpan
        private string FormatarTempoDecorrido(TimeSpan duracao)
        {
            // Se for zero ou negativo, retorna "Agora" ou "0s"
            if (duracao.TotalSeconds <= 0) return "< 1m"; // Ou "Agora", ou "0s"

            var sb = new StringBuilder();
            // Dias
            if (duracao.Days > 0) sb.Append($"{duracao.Days}d ");
            // Horas (só se houver dias ou se for mais de 0 horas)
            if (duracao.Days > 0 || duracao.Hours > 0)
                if (duracao.Hours > 0) sb.Append($"{duracao.Hours}h "); // Mostra 0h se houver dias
            // Minutos (só se houver dias/horas ou se for mais de 0 minutos)
            if (duracao.TotalHours > 0 || duracao.Minutes > 0)
                if (duracao.Minutes > 0) sb.Append($"{duracao.Minutes}m");

            // Se depois de tudo ainda estiver vazio (ex: 30 segundos), mostra minutos aproximados
            if (sb.Length == 0)
            {
                // Mostra '< 1m' para durações curtas
                return "< 1m";
                // Alternativa: Mostrar segundos
                // return $"{Math.Max(1, (int)duracao.TotalSeconds)}s";
            }

            return sb.ToString().Trim();
        }

        // Limpa os detalhes
        private void LimparDetalhes()
        {
            if (this.InvokeRequired) { this.Invoke(new Action(LimparDetalhes)); return; }
            lblValorSenderId.Text = string.Empty;
            lblValorStatus.Text = string.Empty;
            lblValorLastUpdate.Text = string.Empty; // Label usado para Data Criação
            lblValorTempoAberto.Text = string.Empty; // Limpa label de tempo aberto
            btnFinalizarSelecionada.Enabled = false; // Garante que botão está desabilitado
        }

        // Mostra detalhes da conversa selecionada
        private void MostrarDetalhesConversaSelecionada(ConversationStatusApi conv)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => MostrarDetalhesConversaSelecionada(conv))); return; }
            if (conv == null) { LimparDetalhes(); return; }

            // Atualiza os labels existentes
            lblValorSenderId.Text = !string.IsNullOrWhiteSpace(conv.ContactName) ? $"{conv.ContactName} ({conv.SenderId})" : conv.SenderId;
            lblValorStatus.Text = conv.Status;
            lblValorLastUpdate.Text = conv.CreationDateTime.ToString("dd/MM/yyyy HH:mm:ss"); // Mostra Data Criação

            // --- Calcular e exibir Tempo Aberto ---
            TimeSpan duracao = TimeSpan.Zero; // Valor padrão
            string tempoAbertoFormatado = "N/A"; // Valor padrão

            if (conv.Status != null && conv.Status.Equals("open", StringComparison.OrdinalIgnoreCase))
            {
                // Para conversas abertas, calcula desde a criação até agora
                duracao = DateTime.Now - conv.CreationDateTime;
                tempoAbertoFormatado = FormatarTempoDecorrido(duracao);
            }
            else if (conv.ClosedDateTime.HasValue) // Para conversas fechadas com data de fechamento
            {
                // Calcula desde a criação até o fechamento
                duracao = conv.ClosedDateTime.Value - conv.CreationDateTime;
                tempoAbertoFormatado = FormatarTempoDecorrido(duracao);
            }
            // else: Mantém "N/A" se for fechada mas sem data de fechamento (improvável)

            // Atualiza o novo label de tempo aberto
            // Certifique-se que o controle lblValorTempoAberto existe no Designer.cs!
            lblValorTempoAberto.Text = tempoAbertoFormatado;
        }


        // Remove item da lista de Abertas pelo SenderId
        private void RemoverItemListViewAbertas(string senderId)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => RemoverItemListViewAbertas(senderId))); return; }
            for (int i = listViewAbertasParaFinalizar.Items.Count - 1; i >= 0; i--)
            {
                ListViewItem item = listViewAbertasParaFinalizar.Items[i];
                if (item.Tag is ConversationStatusApi conv && conv.SenderId == senderId)
                {
                    listViewAbertasParaFinalizar.Items.RemoveAt(i);
                    return;
                }
            }
        }

        // --- Tratamento de Erros (Helper) ---
        private void MostrarErro(string titulo, Exception? ex = null, string? apiMensagem = null)
        {
            string mensagemCompleta = titulo;
            if (!string.IsNullOrWhiteSpace(apiMensagem))
            {
                // Tenta extrair mensagem de erro do JSON, se possível
                string erroApiDetalhe = apiMensagem;
                try
                {
                    using (JsonDocument document = JsonDocument.Parse(apiMensagem))
                    {
                        if (document.RootElement.TryGetProperty("error", out JsonElement errorElement))
                        {
                            erroApiDetalhe = errorElement.GetString() ?? apiMensagem;
                        }
                        else if (document.RootElement.TryGetProperty("message", out JsonElement msgElement))
                        {
                            erroApiDetalhe = msgElement.GetString() ?? apiMensagem;
                        }
                    }
                }
                catch { /* Ignora se não for JSON válido */ }
                mensagemCompleta += $"\n\nAPI: {erroApiDetalhe}";
            }
            if (ex != null)
            {
                var innerEx = ex;
                while (innerEx.InnerException != null) innerEx = innerEx.InnerException;
                mensagemCompleta += $"\n\nTécnico ({innerEx.GetType().Name}): {innerEx.Message}";
            }
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => MessageBox.Show(this, mensagemCompleta, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error)));
            }
            else
            {
                MessageBox.Show(this, mensagemCompleta, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Console.WriteLine($"ERRO: {mensagemCompleta}");
        }

        // --- Event Handlers ---

        // Evento de seleção na lista de Abertas
        private void listViewAbertasParaFinalizar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewAbertasParaFinalizar.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewAbertasParaFinalizar.SelectedItems[0];
                if (selectedItem.Tag is ConversationStatusApi conv)
                {
                    MostrarDetalhesConversaSelecionada(conv);
                    btnFinalizarSelecionada.Enabled = true; // Habilita finalizar para abertas
                }
                else { LimparDetalhes(); Console.WriteLine("WARN: Tag inválido na seleção (Abertas)."); }
            }
            else { LimparDetalhes(); } // LimparDetalhes já desabilita o botão
        }

        // Evento de seleção na lista de Encerradas
        private void listViewEncerradas_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewEncerradas.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewEncerradas.SelectedItems[0];
                if (selectedItem.Tag is ConversationStatusApi conv)
                {
                    MostrarDetalhesConversaSelecionada(conv);
                    // NÃO habilita o botão finalizar aqui
                }
                else { LimparDetalhes(); Console.WriteLine("WARN: Tag inválido na seleção (Encerradas)."); }
            }
            else { LimparDetalhes(); }
        }

        // Evento de clique no botão Finalizar
        private async void btnFinalizarSelecionada_Click(object sender, EventArgs e)
        {
            if (listViewAbertasParaFinalizar.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewAbertasParaFinalizar.SelectedItems[0];
                if (selectedItem.Tag is ConversationStatusApi convParaFinalizar)
                {
                    DialogResult confirmacao = MessageBox.Show(
                        $"Finalizar conversa com {(!string.IsNullOrWhiteSpace(convParaFinalizar.ContactName) ? convParaFinalizar.ContactName : convParaFinalizar.SenderId)}?", // Mostra nome ou ID
                        "Confirmar Finalização",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (confirmacao == DialogResult.Yes) { await FinalizarConversaApiAsync(convParaFinalizar.SenderId); }
                }
                else { MessageBox.Show("Erro ao obter dados da conversa selecionada.", "Erro Interno", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            }
            else { MessageBox.Show("Nenhuma conversa selecionada para finalizar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        }

        // Evento de clique botão Atualizar Visão Geral (button1)
        private async void button1_Click(object sender, EventArgs e)
        {
            IniciarCarregamento();
            try { await CarregarContagensAsync(); }
            catch (Exception ex) { MostrarErro("Erro ao atualizar contagens", ex); }
            finally { FinalizarCarregamento(); }
        }

        // Evento de clique botão Atualizar Encerradas
        private async void btnAtualizarEncerradas_Click(object sender, EventArgs e)
        {
            IniciarCarregamento();
            try { await CarregarConversasEncerradasAsync(); }
            catch (Exception ex) { MostrarErro("Erro ao atualizar lista de conversas encerradas", ex); }
            finally { FinalizarCarregamento(); }
        }

        // Método para liberar recursos ao fechar o formulário 
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

        }

        // ------ Classes DTO ------

        // Para desserializar a resposta de GET /count
        public class ContagemConversasApi
        {
            [System.Text.Json.Serialization.JsonPropertyName("new_conversation_count")]
            public int ContagemNovas { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("open_conversation_count")]
            public int ContagemAbertas { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("closed_conversation_count")]
            public int ContagemEncerradas { get; set; }
        }

        // Para desserializar cada entrada na resposta de GET /status
        public class ConversationStatusApi
        {
            [System.Text.Json.Serialization.JsonPropertyName("sender_id")]
            public required string SenderId { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("status")]
            public required string Status { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("contact_name")]
            public string? ContactName { get; set; } // Nome pode ser nulo

            [System.Text.Json.Serialization.JsonPropertyName("creation_timestamp")]
            public long CreationTimestamp { get; set; } // Unix timestamp (long)

            [System.Text.Json.Serialization.JsonPropertyName("closed_timestamp")]
            public long? ClosedTimestamp { get; set; } // Unix timestamp (long), pode ser NULL

            // --- Propriedades Auxiliares C# ---
            [System.Text.Json.Serialization.JsonIgnore] // Não mapear do JSON
            public DateTime CreationDateTime => DateTimeOffset.FromUnixTimeSeconds(CreationTimestamp).LocalDateTime;

            [System.Text.Json.Serialization.JsonIgnore] // Não mapear do JSON
            public DateTime? ClosedDateTime => ClosedTimestamp.HasValue
                                                ? DateTimeOffset.FromUnixTimeSeconds(ClosedTimestamp.Value).LocalDateTime
                                                : (DateTime?)null; // Retorna null se ClosedTimestamp for null
        }

        // Para desserializar a resposta de POST /close/{sender_id}
        public class CloseStatusApi
        {
            [System.Text.Json.Serialization.JsonPropertyName("status")]
            public required string Status { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("error")]
            public string Error { get; set; }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        // Evento de clique botão Atualizar SOMENTE Conversas Abertas
        private async void btnAtualizarAbertas_Click(object sender, EventArgs e)
        {
            // Feedback visual específico para esta ação
            this.Cursor = Cursors.WaitCursor;
            Button? botaoClicado = sender as Button; // Pega referência ao botão clicado
            if (botaoClicado != null) botaoClicado.Enabled = false; // Desabilita o próprio botão
            btnFinalizarSelecionada.Enabled = false; // Desabilita finalizar durante a carga
            LimparDetalhes(); // Limpa o painel de detalhes

            // IMPORTANTE: Não precisa limpar a lista aqui, CarregarConversasAbertasAsync já faz isso.

            try
            {
                // Chama diretamente o método que carrega e popula a lista de abertas
                // Este método já busca, filtra e chama PopularListViewAbertas (que limpa a lista)
                await CarregarConversasAbertasAsync();
            }
            catch (Exception ex)
            {
                // Mostra erro se falhar ao carregar/processar conversas abertas
                MostrarErro("Erro ao atualizar lista de conversas abertas", ex);
            }
            finally
            {
                // Restaura o cursor e o botão de atualizar abertas
                this.Cursor = Cursors.Default;
                if (botaoClicado != null) botaoClicado.Enabled = true;
                // O botão Finalizar continuará desabilitado até uma nova seleção
            }
        }

        private void lblValorTempoAberto_Click(object sender, EventArgs e)
        {

        }
    }
} // Fim do namespace



