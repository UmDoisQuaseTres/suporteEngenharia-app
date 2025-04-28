using System;
using System.Collections.Generic;
using System.Linq; // Necessário para .Where() e .ToList()
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization; // Para [JsonPropertyName]
using System.Threading.Tasks;
using System.Windows.Forms;

namespace suporteEngenhariaUI // Certifique-se que este é o namespace correto
{
    public partial class Form1 : Form // Sua classe de formulário (PRIMEIRA classe no arquivo)
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
            InitializeComponent(); // Essencial

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

            // --- Conectar Eventos ---
            // É MELHOR conectar eventos pelo Designer (Janela de Propriedades -> Eventos ⚡).
            // Se você conectou pelo designer, estas linhas NÃO são necessárias e podem causar duplicação.
            // Se você NÃO conectou pelo designer, descomente as linhas correspondentes.

            // this.Load += new System.EventHandler(this.Form1_Load); // Conectado no Designer.cs por padrão
            // this.listViewAbertasParaFinalizar.SelectedIndexChanged += new System.EventHandler(this.listViewAbertasParaFinalizar_SelectedIndexChanged); // Conectado no Designer.cs
            // this.btnFinalizarSelecionada.Click += new System.EventHandler(this.btnFinalizarSelecionada_Click); // Conectado no Designer.cs
            // this.button1.Click += new System.EventHandler(this.button1_Click); // Conecte no Designer se necessário
            // this.btnAtualizarEncerradas.Click += new System.EventHandler(this.btnAtualizarEncerradas_Click); // Conecte no Designer se adicionou o botão
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
            this.Cursor = Cursors.WaitCursor;
            listViewAbertasParaFinalizar.Items.Clear();
            listViewEncerradas.Items.Clear();
            LimparDetalhes();
            btnFinalizarSelecionada.Enabled = false;

            try
            {
                Task contagensTask = CarregarContagensAsync();
                Task abertasTask = CarregarConversasAbertasAsync();
                Task encerradasTask = CarregarConversasEncerradasAsync();
                await Task.WhenAll(contagensTask, abertasTask, encerradasTask);
            }
            catch (Exception ex) { MostrarErro("Erro geral ao carregar dados", ex); }
            finally { this.Cursor = Cursors.Default; }
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
                        AtualizarLabelsContagem(contagens.ContagemAbertas, contagens.ContagemAbertas, contagens.ContagemEncerradas);
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
            }
        }

        // --- Busca Dados API: Conversas Abertas ---
        private async Task CarregarConversasAbertasAsync()
        {
            listViewAbertasParaFinalizar.Items.Clear();
            try
            {
                HttpResponseMessage response = await client.GetAsync(ApiEndpointStatuses);
                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var todosStatus = JsonSerializer.Deserialize<Dictionary<string, ConversationStatusApi>>(jsonString, options);

                    if (todosStatus != null)
                    {
                        List<ConversationStatusApi> conversasAbertas = todosStatus.Values
                            .Where(conv => conv.Status != null && conv.Status.Equals("open", StringComparison.OrdinalIgnoreCase))
                            .OrderByDescending(conv => conv.CreationTimestamp) // <-- CORRIGIDO AQUI
                            .ToList();
                        PopularListViewAbertas(conversasAbertas);
                    }
                }
                else { MostrarErro($"Erro HTTP {response.StatusCode} ao buscar status (para abertas)", null, await response.Content.ReadAsStringAsync()); }
            }
            catch (Exception ex) { MostrarErro("Erro ao buscar status (para abertas)", ex); }
        }

        // --- Busca Dados API: Conversas Encerradas ---
        private async Task CarregarConversasEncerradasAsync()
        {
            listViewEncerradas.Items.Clear();
            try
            {
                HttpResponseMessage response = await client.GetAsync(ApiEndpointStatuses);
                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var todosStatus = JsonSerializer.Deserialize<Dictionary<string, ConversationStatusApi>>(jsonString, options);

                    if (todosStatus != null)
                    {
                        List<ConversationStatusApi> conversasEncerradas = todosStatus.Values
                            .Where(conv => conv.Status != null && conv.Status.Equals("closed", StringComparison.OrdinalIgnoreCase))
                            .OrderByDescending(conv => conv.CreationTimestamp) // <-- CORRIGIDO AQUI (Ordena por criação, pode mudar para ClosedTimestamp se preferir)
                            .ToList();
                        PopularListViewEncerradas(conversasEncerradas);
                    }
                }
                else { MostrarErro($"Erro HTTP {response.StatusCode} ao buscar status (para encerradas)", null, await response.Content.ReadAsStringAsync()); }
            }
            catch (Exception ex) { MostrarErro("Erro ao buscar status (para encerradas)", ex); }
        }


        // --- Ação API: Finalizar Conversa ---
        private async Task FinalizarConversaApiAsync(string senderId)
        {
            if (string.IsNullOrEmpty(senderId)) return;

            this.Cursor = Cursors.WaitCursor;
            btnFinalizarSelecionada.Enabled = false;
            try
            {
                string endpointFinalizar = string.Format(ApiEndpointClose, senderId);
                HttpResponseMessage response = await client.PostAsync(endpointFinalizar, null);

                string responseBody = await response.Content.ReadAsStringAsync();
                CloseStatusApi statusResult = null;
                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    statusResult = JsonSerializer.Deserialize<CloseStatusApi>(responseBody, options);
                }
                catch { /* Ignora */ }


                if (response.IsSuccessStatusCode && statusResult?.Status == "closed")
                {
                    MessageBox.Show($"Conversa com {senderId} finalizada com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RemoverItemListViewAbertas(senderId);
                    LimparDetalhes();
                    await CarregarContagensAsync();
                    await CarregarConversasEncerradasAsync();
                }
                else if (statusResult?.Status == "already_closed")
                {
                    MessageBox.Show($"Conversa com {senderId} já estava finalizada.", "Informação", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RemoverItemListViewAbertas(senderId);
                    LimparDetalhes();
                    await CarregarContagensAsync();
                    await CarregarConversasEncerradasAsync();
                }
                else if (statusResult?.Status == "not_found")
                {
                    MessageBox.Show($"Conversa com {senderId} não encontrada na API.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    RemoverItemListViewAbertas(senderId);
                    LimparDetalhes();
                    await CarregarContagensAsync();
                    await CarregarConversasEncerradasAsync();
                }
                else
                {
                    MostrarErro($"Falha ao finalizar conversa com {senderId}. Status: {response.StatusCode}", null, responseBody);
                    btnFinalizarSelecionada.Enabled = listViewAbertasParaFinalizar.SelectedItems.Count > 0;
                }
            }
            catch (Exception ex)
            {
                MostrarErro($"Erro de conexão ao finalizar conversa com {senderId}", ex);
                btnFinalizarSelecionada.Enabled = listViewAbertasParaFinalizar.SelectedItems.Count > 0;
            }
            finally { this.Cursor = Cursors.Default; }
        }

        // --- Métodos de Atualização da UI ---

        private void AtualizarLabelsContagem(int novasAbertas, int abertas, int encerradas)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => AtualizarLabelsContagem(novasAbertas, abertas, encerradas))); return; }
            lblContagemNovas.Text = novasAbertas < 0 ? "Novas: Erro" : $"Novas/Abertas: {novasAbertas}";
            lblContagemAbertas.Text = abertas < 0 ? "Em Aberto: Erro" : $"Em Aberto: {abertas}";
            lblContagemEncerradas.Text = encerradas < 0 ? "Encerradas: Erro" : $"Encerradas: {encerradas}";
        }

        // Popula ListView de Abertas
        private void PopularListViewAbertas(List<ConversationStatusApi> conversas)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => PopularListViewAbertas(conversas))); return; }
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
            // Opcional: Ajustar colunas
            // listViewAbertasParaFinalizar.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listViewAbertasParaFinalizar.EndUpdate();
        }

        // Popula ListView de Encerradas
        private void PopularListViewEncerradas(List<ConversationStatusApi> conversas)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => PopularListViewEncerradas(conversas))); return; }
            listViewEncerradas.BeginUpdate();
            listViewEncerradas.Items.Clear();
            if (conversas != null)
            {
                foreach (var conv in conversas)
                {
                    ListViewItem item = new ListViewItem(conv.SenderId); // Col 1
                    item.SubItems.Add(conv.CreationDateTime.ToString("dd/MM/yy HH:mm:ss")); // Col 2 (Data Criação)
                    item.SubItems.Add(conv.Status ?? ""); // Col 3 ("closed")
                    // Coluna 4 (Data Fechamento) - ADICIONE esta coluna no Designer!
                    item.SubItems.Add(conv.ClosedDateTime?.ToString("dd/MM/yy HH:mm:ss") ?? "N/A");
                    item.Tag = conv;
                    listViewEncerradas.Items.Add(item);
                }
            }
            // Opcional: Ajustar colunas
            // listViewEncerradas.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listViewEncerradas.EndUpdate();
        }


        // Limpa os detalhes
        private void LimparDetalhes()
        {
            if (this.InvokeRequired) { this.Invoke(new Action(LimparDetalhes)); return; }
            // Usa os nomes corretos dos labels de valor (conforme Designer.cs)
            lblValorSenderId.Text = string.Empty;
            lblValorStatus.Text = string.Empty;
            lblValorLastUpdate.Text = string.Empty; // Este label agora mostra CreationDateTime
            // Limpa/oculta campo de texto se você o manteve
            // txtDetalheCorpoMensagem.Text = string.Empty;
            // txtDetalheCorpoMensagem.Visible = false;
            // lblDescCorpo.Visible = false;
        }

        // Mostra detalhes da conversa selecionada
        private void MostrarDetalhesConversaSelecionada(ConversationStatusApi conv)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => MostrarDetalhesConversaSelecionada(conv))); return; }
            if (conv == null) { LimparDetalhes(); return; }

            // Usa os nomes corretos dos labels de valor
            lblValorSenderId.Text = conv.SenderId;
            lblValorStatus.Text = conv.Status;
            lblValorLastUpdate.Text = conv.CreationDateTime.ToString("g"); // Mostra data/hora CRIO

            // Mostra placeholder no campo de texto (se você o manteve)
            // txtDetalheCorpoMensagem.Text = "(Conteúdo da mensagem não disponível nesta visualização)";
            // txtDetalheCorpoMensagem.Visible = true;
            // lblDescCorpo.Visible = true;
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
        private void MostrarErro(string titulo, Exception ex = null, string apiMensagem = null)
        {
            string mensagemCompleta = titulo;
            if (!string.IsNullOrWhiteSpace(apiMensagem)) { mensagemCompleta += $"\n\nAPI: {apiMensagem}"; }
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
                else { LimparDetalhes(); btnFinalizarSelecionada.Enabled = false; Console.WriteLine("WARN: Tag inválido."); }
            }
            else { LimparDetalhes(); btnFinalizarSelecionada.Enabled = false; }
        }

        // Evento de clique no botão Finalizar
        private async void btnFinalizarSelecionada_Click(object sender, EventArgs e)
        {
            if (listViewAbertasParaFinalizar.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewAbertasParaFinalizar.SelectedItems[0];
                if (selectedItem.Tag is ConversationStatusApi convParaFinalizar)
                {
                    DialogResult confirmacao = MessageBox.Show($"Finalizar conversa com {convParaFinalizar.SenderId}?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (confirmacao == DialogResult.Yes)
                    {
                        await FinalizarConversaApiAsync(convParaFinalizar.SenderId);
                    }
                }
                else { MessageBox.Show("Erro ao obter dados da conversa.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            }
            else { MessageBox.Show("Selecione uma conversa para finalizar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        }

        // Evento de clique botão Atualizar Visão Geral (button1)
        private async void button1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            button1.Enabled = false;
            try
            {
                await CarregarContagensAsync();
            }
            catch (Exception ex) { MostrarErro("Erro ao atualizar contagens", ex); }
            finally
            {
                this.Cursor = Cursors.Default;
                button1.Enabled = true;
            }
        }

        // (Opcional) Evento de clique botão Atualizar Encerradas
        // Certifique-se que o botão se chama btnAtualizarEncerradas no Designer
        // e que este evento está conectado a ele.
        private async void btnAtualizarEncerradas_Click(object sender, EventArgs e)
        {
            // Tenta fazer o cast do 'sender' para Button UMA VEZ no início.
            Button botaoClicado = sender as Button;

            this.Cursor = Cursors.WaitCursor;

            // Desabilita o botão ANTES do try, se o cast funcionou.
            if (botaoClicado != null)
            {
                botaoClicado.Enabled = false;
            }

            try
            {
                // Chama o método para carregar os dados.
                await CarregarConversasEncerradasAsync();
            }
            catch (Exception ex)
            {
                // Mostra o erro se a carga falhar.
                MostrarErro("Erro ao atualizar lista de conversas encerradas", ex);
            }
            finally
            {
                // SEMPRE restaura o cursor e reabilita o botão (se for um botão).
                this.Cursor = Cursors.Default;
                if (botaoClicado != null)
                {
                    botaoClicado.Enabled = true; // Reabilita o MESMO botão.
                }
            }
        }

    } // Fim da classe Form1

    // ------ Coloque as classes DTO aqui ou em arquivos separados ------

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
        public string SenderId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string Status { get; set; }

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
        public string Status { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("error")]
        public string Error { get; set; }
    }

} // Fim do namespace