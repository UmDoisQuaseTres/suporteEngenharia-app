using System.Net.Http.Headers;
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
        }

        // --- Evento Load ---
        private async void Form1_Load(object sender, EventArgs e)
        {
            LimparDetalhes();
            btnFinalizarSelecionada.Enabled = false;
            await CarregarDadosIniciaisAsync();
        }

        // --- Carregamento Principal ---
        private async Task CarregarDadosIniciaisAsync()
        {
            IniciarCarregamento();

            try
            {
                // Executa todas as tarefas de carregamento em paralelo
                await Task.WhenAll(
                    CarregarContagensAsync(),
                    CarregarConversasAbertasAsync(),
                    CarregarConversasEncerradasAsync()
                );
            }
            catch (Exception ex)
            {
                MostrarErro("Erro geral ao carregar dados", ex);
            }
            finally
            {
                FinalizarCarregamento();
            }
        }

        // Método auxiliar para iniciar estado de carregamento
        private void IniciarCarregamento()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(IniciarCarregamento));
                return;
            }

            this.Cursor = Cursors.WaitCursor;
            btnFinalizarSelecionada.Enabled = false;
            btnAtualizarEncerradas.Enabled = false;
            button1.Enabled = false; // Botão de atualizar visão geral

            // Limpa listas para mostrar que estão sendo recarregadas
            listViewAbertasParaFinalizar.Items.Clear();
            listViewEncerradas.Items.Clear();
            LimparDetalhes();
        }

        // Método auxiliar para finalizar estado de carregamento
        private void FinalizarCarregamento()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(FinalizarCarregamento));
                return;
            }

            this.Cursor = Cursors.Default;
            btnAtualizarEncerradas.Enabled = true;
            button1.Enabled = true; // Botão de atualizar visão geral
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
                        // Corrigido: Primeiro parâmetro agora usa ContagemNovas em vez de duplicar ContagemAbertas
                        AtualizarLabelsContagem(contagens.ContagemNovas, contagens.ContagemAbertas, contagens.ContagemEncerradas);
                    }
                }
                else
                {
                    Console.WriteLine($"Erro HTTP {response.StatusCode} ao buscar contagens.");
                    AtualizarLabelsContagem(-1, -1, -1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar contagens: {ex.Message}");
                AtualizarLabelsContagem(-1, -1, -1);
                throw; // Re-lança a exceção para ser capturada no método chamador
            }
        }

        // --- Busca Dados API: Conversas ---
        // Método base para evitar duplicação de código
        private async Task<Dictionary<string, ConversationStatusApi>?> BuscarStatusConversoesAsync()
        {
            HttpResponseMessage response = await client.GetAsync(ApiEndpointStatuses);

            if (!response.IsSuccessStatusCode)
            {
                MostrarErro($"Erro HTTP {response.StatusCode} ao buscar status de conversas",
                    null, await response.Content.ReadAsStringAsync());
                return null;
            }

            string jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<Dictionary<string, ConversationStatusApi>>(jsonString, options);
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
                    PopularListViewAbertas(conversasAbertas);
                }
            }
            catch (Exception ex)
            {
                MostrarErro("Erro ao processar conversas abertas", ex);
                throw; // Re-lança a exceção para ser capturada no método chamador
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
                        .OrderByDescending(conv => conv.ClosedTimestamp ?? conv.CreationTimestamp) // Melhorado: Usa ClosedTimestamp se disponível
                        .ToList();
                    PopularListViewEncerradas(conversasEncerradas);
                }
            }
            catch (Exception ex)
            {
                MostrarErro("Erro ao processar conversas encerradas", ex);
                throw; // Re-lança a exceção para ser capturada no método chamador
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
                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    statusResult = JsonSerializer.Deserialize<CloseStatusApi>(responseBody, options);
                }
                catch { /* Ignora falhas de desserialização */ }

                if (response.IsSuccessStatusCode && statusResult?.Status == "closed")
                {
                    MessageBox.Show($"Conversa com {senderId} finalizada com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RemoverItemListViewAbertas(senderId);
                    LimparDetalhes();

                    // Atualiza dados para refletir a mudança
                    await Task.WhenAll(
                        CarregarContagensAsync(),
                        CarregarConversasEncerradasAsync()
                    );
                }
                else if (statusResult?.Status == "already_closed")
                {
                    MessageBox.Show($"Conversa com {senderId} já estava finalizada.", "Informação", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RemoverItemListViewAbertas(senderId);
                    LimparDetalhes();

                    // Atualiza dados para refletir a mudança
                    await Task.WhenAll(
                        CarregarContagensAsync(),
                        CarregarConversasEncerradasAsync()
                    );
                }
                else if (statusResult?.Status == "not_found")
                {
                    MessageBox.Show($"Conversa com {senderId} não encontrada na API.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    RemoverItemListViewAbertas(senderId);
                    LimparDetalhes();

                    // Atualiza dados para refletir a mudança
                    await Task.WhenAll(
                        CarregarContagensAsync(),
                        CarregarConversasEncerradasAsync()
                    );
                }
                else
                {
                    MostrarErro($"Falha ao finalizar conversa com {senderId}. Status: {response.StatusCode}", null, responseBody);
                }
            }
            catch (Exception ex)
            {
                MostrarErro($"Erro de conexão ao finalizar conversa com {senderId}", ex);
            }
            finally
            {
                FinalizarCarregamento();
                // Restaura botão de finalizar se uma linha estiver selecionada
                btnFinalizarSelecionada.Enabled = listViewAbertasParaFinalizar.SelectedItems.Count > 0;
            }
        }

        // --- Métodos de Atualização da UI ---

        private void AtualizarLabelsContagem(int novas, int abertas, int encerradas)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => AtualizarLabelsContagem(novas, abertas, encerradas)));
                return;
            }

            lblContagemNovas.Text = novas < 0 ? "Novas: Erro" : $"Novas: {novas}";
            lblContagemAbertas.Text = abertas < 0 ? "Em Aberto: Erro" : $"Em Aberto: {abertas}";
            lblContagemEncerradas.Text = encerradas < 0 ? "Encerradas: Erro" : $"Encerradas: {encerradas}";
        }

        // Popula ListView de Abertas
        private void PopularListViewAbertas(List<ConversationStatusApi> conversas)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => PopularListViewAbertas(conversas)));
                return;
            }

            listViewAbertasParaFinalizar.BeginUpdate();
            listViewAbertasParaFinalizar.Items.Clear();

            if (conversas != null)
            {
                foreach (var conv in conversas)
                {
                    ListViewItem item = new ListViewItem(conv.SenderId); // Col 1
                    item.SubItems.Add(conv.CreationDateTime.ToString("dd/MM/yy HH:mm:ss")); // Col 2 (Data Criação)
                    item.SubItems.Add(conv.Status ?? ""); // Col 3
                    item.Tag = conv;
                    listViewAbertasParaFinalizar.Items.Add(item);
                }
            }

            // Ajusta tamanho das colunas para caber o conteúdo
            listViewAbertasParaFinalizar.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewAbertasParaFinalizar.EndUpdate();
        }

        // Popula ListView de Encerradas
        private void PopularListViewEncerradas(List<ConversationStatusApi> conversas)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => PopularListViewEncerradas(conversas)));
                return;
            }

            listViewEncerradas.BeginUpdate();
            listViewEncerradas.Items.Clear();

            if (conversas != null)
            {
                foreach (var conv in conversas)
                {
                    ListViewItem item = new ListViewItem(conv.SenderId); // Col 1
                    item.SubItems.Add(conv.CreationDateTime.ToString("dd/MM/yy HH:mm:ss")); // Col 2 (Data Criação)
                    item.SubItems.Add(conv.Status ?? ""); // Col 3 ("closed")
                    // Coluna 4 (Data Fechamento)
                    item.SubItems.Add(conv.ClosedDateTime?.ToString("dd/MM/yy HH:mm:ss") ?? "N/A");
                    item.Tag = conv;
                    listViewEncerradas.Items.Add(item);
                }
            }

            // Ajusta tamanho das colunas para caber o conteúdo
            listViewEncerradas.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewEncerradas.EndUpdate();
        }

        // Limpa os detalhes
        private void LimparDetalhes()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(LimparDetalhes));
                return;
            }

            lblValorSenderId.Text = string.Empty;
            lblValorStatus.Text = string.Empty;
            lblValorLastUpdate.Text = string.Empty;

            // Desabilita o botão de finalizar quando limpa os detalhes
            btnFinalizarSelecionada.Enabled = false;
        }

        // Mostra detalhes da conversa selecionada
        private void MostrarDetalhesConversaSelecionada(ConversationStatusApi conv)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => MostrarDetalhesConversaSelecionada(conv)));
                return;
            }

            if (conv == null)
            {
                LimparDetalhes();
                return;
            }

            lblValorSenderId.Text = conv.SenderId;
            lblValorStatus.Text = conv.Status;
            lblValorLastUpdate.Text = conv.CreationDateTime.ToString("dd/MM/yyyy HH:mm:ss");
        }

        // Remove item da lista de Abertas pelo SenderId
        private void RemoverItemListViewAbertas(string senderId)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => RemoverItemListViewAbertas(senderId)));
                return;
            }

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
                mensagemCompleta += $"\n\nAPI: {apiMensagem}";
            }

            if (ex != null)
            {
                var innerEx = ex;
                while (innerEx.InnerException != null) innerEx = innerEx.InnerException;
                mensagemCompleta += $"\n\nTécnico: {innerEx.Message}";
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
                    btnFinalizarSelecionada.Enabled = true;
                }
                else
                {
                    LimparDetalhes();
                    Console.WriteLine("WARN: Tag inválido na seleção.");
                }
            }
            else
            {
                LimparDetalhes();
            }
        }

        // Evento de seleção na lista de Encerradas (para mostrar detalhes)
        private void listViewEncerradas_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewEncerradas.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewEncerradas.SelectedItems[0];
                if (selectedItem.Tag is ConversationStatusApi conv)
                {
                    MostrarDetalhesConversaSelecionada(conv);
                    // Não habilita botão de finalizar para conversas já encerradas
                }
                else
                {
                    LimparDetalhes();
                    Console.WriteLine("WARN: Tag inválido na seleção de encerradas.");
                }
            }
            else
            {
                LimparDetalhes();
            }
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
                        $"Finalizar conversa com {convParaFinalizar.SenderId}?",
                        "Confirmar",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (confirmacao == DialogResult.Yes)
                    {
                        await FinalizarConversaApiAsync(convParaFinalizar.SenderId);
                    }
                }
                else
                {
                    MessageBox.Show("Erro ao obter dados da conversa.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Selecione uma conversa para finalizar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Evento de clique botão Atualizar Visão Geral (button1)
        private async void button1_Click(object sender, EventArgs e)
        {
            IniciarCarregamento();
            try
            {
                await CarregarContagensAsync();
            }
            catch (Exception ex)
            {
                MostrarErro("Erro ao atualizar contagens", ex);
            }
            finally
            {
                FinalizarCarregamento();
            }
        }


        // Evento de clique botão Atualizar Encerradas
        private async void btnAtualizarEncerradas_Click(object sender, EventArgs e)
        {
            IniciarCarregamento();
            try
            {
                await CarregarConversasEncerradasAsync();
            }
            catch (Exception ex)
            {
                MostrarErro("Erro ao atualizar lista de conversas encerradas", ex);
            }
            finally
            {
                FinalizarCarregamento();
            }
        }


        // Método para liberar recursos ao fechar o formulário
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
           
        }

    } // Fim da classe Form1

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
        public required string Error { get; set; }
    }

} // Fim do namespace