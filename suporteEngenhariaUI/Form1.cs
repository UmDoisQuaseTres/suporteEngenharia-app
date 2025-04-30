using System.Net.Http.Headers;
using System.Text; // Necessário para StringBuilder
using System.Text.Json;

namespace suporteEngenhariaUI 
{
    public partial class Form1 : Form 
    {
        // --- HttpClient Configuração ---
        private static readonly HttpClient client = new HttpClient();

        // !! AJUSTE A URL BASE SE NECESSÁRIO !!
        private const string ApiBaseUrl = "http://127.0.0.1:5000/";

        // --- ENDPOINTS DA API PYTHON ---
        private const string ApiEndpointContagens = "count"; // GET
        private const string ApiEndpointStatuses = "status"; // GET
        private const string ApiEndpointClose = "close/{0}"; // POST (com sender_id)

        // --- Construtor ---
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

            // Conexões de Eventos (Melhor feitas pelo Designer)
            this.Load += new System.EventHandler(this.Form1_Load);
            this.listViewAbertasParaFinalizar.SelectedIndexChanged += new System.EventHandler(this.listViewAbertasParaFinalizar_SelectedIndexChanged);
            this.listViewEncerradas.SelectedIndexChanged += new System.EventHandler(this.listViewEncerradas_SelectedIndexChanged); // Adicionado
            this.btnFinalizarSelecionada.Click += new System.EventHandler(this.btnFinalizarSelecionada_Click);
            this.button1.Click += new System.EventHandler(this.button1_Click);
            this.btnAtualizarEncerradas.Click += new System.EventHandler(this.btnAtualizarEncerradas_Click);
            this.btnAtualizarAbertas.Click += new System.EventHandler(this.btnAtualizarAbertas_Click); // Conecta botão Atualizar Abertas
        }

        // --- Evento Load ---
        private async void Form1_Load(object? sender, EventArgs e)
        {
            LimparDetalhes();
            // btnFinalizarSelecionada já é desabilitado por LimparDetalhes
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
        private void IniciarCarregamento()
        {
            if (this.InvokeRequired) { this.Invoke(new Action(IniciarCarregamento)); return; }

            this.Cursor = Cursors.WaitCursor;
            // Desabilita botões de ação/atualização
            btnFinalizarSelecionada.Enabled = false;
            btnAtualizarEncerradas.Enabled = false;
            button1.Enabled = false; // Botão de atualizar visão geral
            btnAtualizarAbertas.Enabled = false; // Botão de atualizar abertas

            // Limpa painel de detalhes (Listas são limpas nos métodos de popular)
            LimparDetalhes();
        }

        private void FinalizarCarregamento()
        {
            if (this.InvokeRequired) { this.Invoke(new Action(FinalizarCarregamento)); return; }

            this.Cursor = Cursors.Default;
            // Reabilita botões de atualização
            btnAtualizarEncerradas.Enabled = true;
            button1.Enabled = true;
            btnAtualizarAbertas.Enabled = true;
            // Botão Finalizar será reabilitado pelo SelectedIndexChanged se necessário
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
                    MostrarErro($"Erro HTTP {response.StatusCode} ao buscar status", null, await response.Content.ReadAsStringAsync());
                    return null;
                }

                string jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<Dictionary<string, ConversationStatusApi>>(jsonString, options);
            }
            catch (JsonException jsonEx)
            {
                MostrarErro("Erro ao desserializar dados de status (JSON inválido?)", jsonEx);
                return null;
            }
            catch (HttpRequestException httpEx)
            {
                MostrarErro("Erro de rede ao buscar status", httpEx);
                return null;
            }
            catch (Exception ex)
            {
                MostrarErro("Erro inesperado ao buscar status", ex);
                throw;
            }
        }

        // --- Busca Dados API: Conversas Abertas ---
        private async Task CarregarConversasAbertasAsync()
        {
            List<ConversationStatusApi> conversasAbertas = new List<ConversationStatusApi>(); // Inicializa vazia
            try
            {
                var todosStatus = await BuscarStatusConversoesAsync();
                if (todosStatus != null)
                {
                    conversasAbertas = todosStatus.Values
                        .Where(conv => conv.Status != null && conv.Status.Equals("open", StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(conv => conv.CreationTimestamp)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro capturado em CarregarConversasAbertasAsync: {ex.Message}");
                // Não mostra erro aqui, pois BuscarStatusConversoesAsync ou CarregarDadosIniciaisAsync já mostraram
                // A lista permanecerá vazia.
                throw; // Re-lança para CarregarDadosIniciaisAsync saber que falhou
            }
            finally // Garante que a UI seja atualizada mesmo em caso de erro na busca/filtro
            {
                PopularListViewAbertas(conversasAbertas); // Popula com a lista (pode estar vazia)
            }
        }

        // --- Busca Dados API: Conversas Encerradas ---
        private async Task CarregarConversasEncerradasAsync()
        {
            List<ConversationStatusApi> conversasEncerradas = new List<ConversationStatusApi>(); // Inicializa vazia
            try
            {
                var todosStatus = await BuscarStatusConversoesAsync();
                if (todosStatus != null)
                {
                    conversasEncerradas = todosStatus.Values
                        .Where(conv => conv.Status != null && conv.Status.Equals("closed", StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(conv => conv.ClosedTimestamp ?? conv.CreationTimestamp)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro capturado em CarregarConversasEncerradasAsync: {ex.Message}");
                // Não mostra erro aqui, pois BuscarStatusConversoesAsync ou CarregarDadosIniciaisAsync já mostraram
                // A lista permanecerá vazia.
                throw;
            }
            finally // Garante que a UI seja atualizada mesmo em caso de erro na busca/filtro
            {
                PopularListViewEncerradas(conversasEncerradas); // Popula com a lista (pode estar vazia)
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
                        // *** Importante: Usa a DTO CORRIGIDA ***
                        statusResult = JsonSerializer.Deserialize<CloseStatusApi>(responseBody, options);
                    }
                    catch (JsonException jsonEx) { Console.WriteLine($"Erro ao desserializar resposta de /close: {jsonEx.Message}"); }
                }

                // Verifica o resultado da operação
                if (response.IsSuccessStatusCode && statusResult?.Status == "closed")
                {
                    MessageBox.Show($"Conversa com {senderId} finalizada com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RemoverItemListViewAbertas(senderId); // Atualiza UI local
                    LimparDetalhes();
                    // Recarrega listas e contagens em background
                    _ = Task.Run(async () => {
                        await Task.Delay(100); // Pequeno delay opcional
                        await CarregarContagensAsync();
                        await CarregarConversasEncerradasAsync(); // Recarrega encerradas para incluir a nova
                    });
                }
                else if (statusResult?.Status == "already_closed")
                {
                    MessageBox.Show($"Conversa com {senderId} já estava finalizada.", "Informação", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RemoverItemListViewAbertas(senderId);
                    LimparDetalhes();
                    _ = Task.Run(async () => {
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
                    _ = Task.Run(async () => {
                        await Task.Delay(100);
                        await CarregarContagensAsync();
                        await CarregarConversasEncerradasAsync();
                    });
                }
                else // Se chegou aqui, algo deu errado (status HTTP não OK ou status JSON inesperado)
                {
                    MostrarErro($"Falha ao finalizar conversa com {senderId}. Status HTTP: {response.StatusCode}", null, responseBody);
                }
            }
            catch (HttpRequestException httpEx) { MostrarErro($"Erro de rede ao finalizar conversa com {senderId}", httpEx); }
            catch (Exception ex) { MostrarErro($"Erro inesperado ao finalizar conversa com {senderId}", ex); }
            finally
            {
                FinalizarCarregamento();
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

            // Salva a seleção atual, se houver
            string? selectedSenderId = null;
            if (listViewAbertasParaFinalizar.SelectedItems.Count > 0 && listViewAbertasParaFinalizar.SelectedItems[0].Tag is ConversationStatusApi selectedConv)
            {
                selectedSenderId = selectedConv.SenderId;
            }

            listViewAbertasParaFinalizar.BeginUpdate();
            listViewAbertasParaFinalizar.Items.Clear();

            if (conversas != null)
            {
                DateTime agora = DateTime.Now;
                foreach (var conv in conversas)
                {
                    string displayIdentifier = !string.IsNullOrWhiteSpace(conv.ContactName) ? conv.ContactName : conv.SenderId;
                    ListViewItem item = new ListViewItem(displayIdentifier);
                    item.SubItems.Add(conv.CreationDateTime.ToString("dd/MM/yy HH:mm:ss"));
                    item.SubItems.Add(conv.Status ?? "");
                    TimeSpan duracaoAberta = agora - conv.CreationDateTime;
                    item.SubItems.Add(FormatarTempoDecorrido(duracaoAberta));
                    item.Tag = conv;
                    listViewAbertasParaFinalizar.Items.Add(item);

                    // Restaura a seleção se o item ainda existir
                    if (conv.SenderId == selectedSenderId)
                    {
                        item.Selected = true;
                        item.Focused = true; // Garante visibilidade
                    }
                }
            }

            //try { listViewAbertasParaFinalizar.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent); }
           // catch { listViewAbertasParaFinalizar.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize); }

            listViewAbertasParaFinalizar.EndUpdate();

            // Garante que o painel de detalhes reflita a seleção (ou falta dela) após a atualização
            if (listViewAbertasParaFinalizar.SelectedItems.Count == 0)
            {
                LimparDetalhes();
            }
            else if (selectedSenderId != null && listViewAbertasParaFinalizar.SelectedItems.Count > 0)
            {
                // Se a seleção foi restaurada, força a atualização dos detalhes
                MostrarDetalhesConversaSelecionada((ConversationStatusApi)listViewAbertasParaFinalizar.SelectedItems[0].Tag);
                btnFinalizarSelecionada.Enabled = true;
            }
        }

        // Popula ListView de Encerradas
        private void PopularListViewEncerradas(List<ConversationStatusApi> conversas)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => PopularListViewEncerradas(conversas))); return; }

            // Salva a seleção atual, se houver
            string? selectedSenderId = null;
            if (listViewEncerradas.SelectedItems.Count > 0 && listViewEncerradas.SelectedItems[0].Tag is ConversationStatusApi selectedConv)
            {
                selectedSenderId = selectedConv.SenderId;
            }


            listViewEncerradas.BeginUpdate();
            listViewEncerradas.Items.Clear();

            if (conversas != null)
            {
                foreach (var conv in conversas)
                {
                    string displayIdentifier = !string.IsNullOrWhiteSpace(conv.ContactName) ? conv.ContactName : conv.SenderId;
                    ListViewItem item = new ListViewItem(displayIdentifier);
                    item.SubItems.Add(conv.CreationDateTime.ToString("dd/MM/yy HH:mm:ss"));
                    item.SubItems.Add(conv.Status ?? "");
                    item.SubItems.Add(conv.ClosedDateTime?.ToString("dd/MM/yy HH:mm:ss") ?? "N/A");
                    TimeSpan duracaoTotal = conv.ClosedDateTime.HasValue ? (conv.ClosedDateTime.Value - conv.CreationDateTime) : TimeSpan.Zero;
                    item.SubItems.Add(FormatarTempoDecorrido(duracaoTotal));
                    item.Tag = conv;
                    listViewEncerradas.Items.Add(item);

                    // Restaura a seleção se o item ainda existir
                    if (conv.SenderId == selectedSenderId)
                    {
                        item.Selected = true;
                        item.Focused = true;
                    }
                }
            }

            //try { listViewEncerradas.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent); }
            //catch { listViewEncerradas.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize); }

            listViewEncerradas.EndUpdate();

            // Garante que o painel de detalhes reflita a seleção (ou falta dela) após a atualização
            if (listViewEncerradas.SelectedItems.Count == 0 && listViewAbertasParaFinalizar.SelectedItems.Count == 0)
            {
                // Só limpa se a outra lista também não tiver seleção
                LimparDetalhes();
            }
            else if (selectedSenderId != null && listViewEncerradas.SelectedItems.Count > 0)
            {
                // Se a seleção foi restaurada, força a atualização dos detalhes
                MostrarDetalhesConversaSelecionada((ConversationStatusApi)listViewEncerradas.SelectedItems[0].Tag);
                // Não habilita o botão finalizar para encerradas
                btnFinalizarSelecionada.Enabled = false;
            }
        }

        // Função auxiliar para formatar TimeSpan
        private string FormatarTempoDecorrido(TimeSpan duracao)
        {
            if (duracao.TotalSeconds <= 0) return "< 1m";
            var sb = new StringBuilder();
            if (duracao.Days > 0) sb.Append($"{duracao.Days}d ");
            if (duracao.Days > 0 || duracao.Hours > 0)
                if (duracao.Hours > 0) sb.Append($"{duracao.Hours}h ");
            if (duracao.TotalHours > 0 || duracao.Minutes > 0)
                if (duracao.Minutes > 0) sb.Append($"{duracao.Minutes}m");
            if (sb.Length == 0) return "< 1m";
            return sb.ToString().Trim();
        }

        // Limpa os detalhes
        private void LimparDetalhes()
        {
            if (this.InvokeRequired) { this.Invoke(new Action(LimparDetalhes)); return; }
            lblValorSenderId.Text = string.Empty;
            lblValorStatus.Text = string.Empty;
            lblValorLastUpdate.Text = string.Empty;
            lblValorTempoAberto.Text = string.Empty;
            btnFinalizarSelecionada.Enabled = false;
        }

        // Mostra detalhes da conversa selecionada
        private void MostrarDetalhesConversaSelecionada(ConversationStatusApi conv)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => MostrarDetalhesConversaSelecionada(conv))); return; }
            if (conv == null) { LimparDetalhes(); return; }

            lblValorSenderId.Text = !string.IsNullOrWhiteSpace(conv.ContactName) ? $"{conv.ContactName} ({conv.SenderId})" : conv.SenderId;
            lblValorStatus.Text = conv.Status;
            lblValorLastUpdate.Text = conv.CreationDateTime.ToString("dd/MM/yyyy HH:mm:ss");

            TimeSpan duracao; string tempoAbertoFormatado;
            if (conv.Status != null && conv.Status.Equals("open", StringComparison.OrdinalIgnoreCase))
            {
                duracao = DateTime.Now - conv.CreationDateTime;
                tempoAbertoFormatado = FormatarTempoDecorrido(duracao);
            }
            else if (conv.ClosedDateTime.HasValue)
            {
                duracao = conv.ClosedDateTime.Value - conv.CreationDateTime;
                tempoAbertoFormatado = FormatarTempoDecorrido(duracao);
            }
            else { tempoAbertoFormatado = "N/A"; }
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
            // ... (código do MostrarErro inalterado) ...
            string mensagemCompleta = titulo;
            if (!string.IsNullOrWhiteSpace(apiMensagem))
            {
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
                catch { /* Ignora */ }
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
        private void listViewAbertasParaFinalizar_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (listViewAbertasParaFinalizar.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewAbertasParaFinalizar.SelectedItems[0];
                if (selectedItem.Tag is ConversationStatusApi conv)
                {
                    MostrarDetalhesConversaSelecionada(conv);
                    btnFinalizarSelecionada.Enabled = true;
                }
                else { LimparDetalhes(); Console.WriteLine("WARN: Tag inválido na seleção (Abertas)."); }
            }
            else { LimparDetalhes(); }
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
                    // Não habilita botão finalizar
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
                        $"Finalizar conversa com {(!string.IsNullOrWhiteSpace(convParaFinalizar.ContactName) ? convParaFinalizar.ContactName : convParaFinalizar.SenderId)}?",
                        "Confirmar Finalização", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

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

        // Evento de clique botão Atualizar Abertas
        private async void btnAtualizarAbertas_Click(object sender, EventArgs e)
        {
            IniciarCarregamento(); // Usa o IniciarCarregamento geral
            try
            {
                await CarregarConversasAbertasAsync();
            }
            catch (Exception ex)
            {
                MostrarErro("Erro ao atualizar lista de conversas abertas", ex);
            }
            finally
            {
                FinalizarCarregamento(); // Usa o FinalizarCarregamento geral
            }
        }

        // Método para liberar recursos ao fechar o formulário
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            // client.Dispose(); // Considerar para aplicações muito longas
        }

        // --- Event Handlers Vazios Gerados pelo Designer (Remova se não conectados) ---
        private void label1_Click(object sender, EventArgs e) { }
        private void lblValorTempoAberto_Click(object sender, EventArgs e) { }


    } // Fim da classe Form1

    // ------ Classes DTO ------
    // (Definições completas devem estar aqui ou em arquivos separados)
    public class ContagemConversasApi
    {
        [System.Text.Json.Serialization.JsonPropertyName("new_conversation_count")]
        public int ContagemNovas { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("open_conversation_count")]
        public int ContagemAbertas { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("closed_conversation_count")]
        public int ContagemEncerradas { get; set; }
    }
    public class ConversationStatusApi
    {
        [System.Text.Json.Serialization.JsonPropertyName("sender_id")]
        public required string SenderId { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public required string Status { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("contact_name")]
        public string? ContactName { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("creation_timestamp")]
        public long CreationTimestamp { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("closed_timestamp")]
        public long? ClosedTimestamp { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public DateTime CreationDateTime => DateTimeOffset.FromUnixTimeSeconds(CreationTimestamp).LocalDateTime;
        [System.Text.Json.Serialization.JsonIgnore]
        public DateTime? ClosedDateTime => ClosedTimestamp.HasValue ? DateTimeOffset.FromUnixTimeSeconds(ClosedTimestamp.Value).LocalDateTime : (DateTime?)null;
    }
    public class CloseStatusApi
    {
        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public required string Status { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("error")]
        public string? Error { get; set; } // Corrigido para ser nullable
    }

} // Fim do namespace