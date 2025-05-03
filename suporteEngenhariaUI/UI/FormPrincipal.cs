using System.Text; // Necessário para StringBuilder
using System.Text.Json;
using suporteEngenhariaUI.Exceptions;
using suporteEngenhariaUI.Models;
using suporteEngenhariaUI.Services;

namespace suporteEngenhariaUI
{
    public partial class FormPrincipal : Form
    {
        // --- Instância do Serviço da API ---
        private readonly WhatsAppApiService _apiService; // Cria uma instância

        // --- Construtor --- //
        public FormPrincipal() => InitializeComponent();
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
            btnAtualizar.Enabled = false;
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
            btnAtualizar.Enabled = true;
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
                // Chama o método do serviço
                ContagemConversasApi? contagens = await _apiService.GetCountsAsync();

                if (contagens != null)
                {
                    AtualizarLabelsContagem(contagens.ContagemNovas, contagens.ContagemAbertas, contagens.ContagemEncerradas);
                }
                else
                {
                    // O serviço agora lança exceção em caso de falha, então este else pode não ser alcançado
                    // Mas é seguro manter para o caso de futuras modificações no serviço
                    Console.WriteLine("API retornou contagens nulas.");
                    AtualizarLabelsContagem(-1, -1, -1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar contagens (capturado no Form): {ex.Message}");
                AtualizarLabelsContagem(-1, -1, -1);
                throw;
            }
        }

        private async Task CarregarConversasAbertasAsync()
        {
            List<ConversationStatusApi> conversasAbertas = new List<ConversationStatusApi>();
            try
            {
                // Chama o método do serviço
                var todosStatus = await _apiService.GetAllStatusesAsync();
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
                Console.WriteLine($"Erro ao carregar conversas abertas (capturado no Form): {ex.Message}");
                throw;
            }
            finally
            {
                PopularListViewAbertas(conversasAbertas); // Popula a UI com o resultado (ou lista vazia)
            }
        }


        // --- Busca Dados API: Conversas Encerradas ---
        private async Task CarregarConversasEncerradasAsync()
        {
            List<ConversationStatusApi> conversasEncerradas = new List<ConversationStatusApi>();
            try
            {
                // Chama o método do serviço
                var todosStatus = await _apiService.GetAllStatusesAsync();

                if (todosStatus != null)
                {
                    conversasEncerradas = todosStatus.Values
                        .Where(conv => conv.Status != null && conv.Status.Equals("closed", StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(conv => conv.ClosedTimestamp ?? conv.CreationTimestamp)
                        .ToList();
                }
            }
            catch (Exception ex) // Captura exceções do serviço
            {
                Console.WriteLine($"Erro ao carregar conversas encerradas (capturado no Form): {ex.Message}");
                // A lista permanecerá vazia
                throw; 
            }
            finally
            {
                PopularListViewEncerradas(conversasEncerradas); // Popula a UI com o resultado (ou lista vazia)
            }
        }

        // --- Ação API: Finalizar Conversa ---
        private async Task FinalizarConversaApiAsync(string senderId)
        {
            if (string.IsNullOrEmpty(senderId)) return;
            IniciarCarregamento();
            try
            {
                // Chama o método do serviço
                CloseStatusApi? statusResult = await _apiService.CloseConversationAsync(senderId);

                // Analisa o resultado retornado pelo serviço
                if (statusResult?.Status == "closed")
                {
                    MessageBox.Show($"Conversa com {senderId} finalizada com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RemoverItemListViewAbertas(senderId);
                    LimparDetalhes();
                    _ = Task.Run(async () => { /* ... recarrega em background ... */ });
                }
                else if (statusResult?.Status == "already_closed")
                {
                    MessageBox.Show($"Conversa com {senderId} já estava finalizada.", "Informação", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RemoverItemListViewAbertas(senderId);
                    LimparDetalhes();
                    _ = Task.Run(async () => { /* ... recarrega ... */ });
                }
                else if (statusResult?.Status == "not_found")
                {
                    MessageBox.Show($"Conversa com {senderId} não encontrada na API.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    RemoverItemListViewAbertas(senderId);
                    LimparDetalhes();
                    _ = Task.Run(async () => { /* ... recarrega ... */ });
                }
                else 
                {
                    MostrarErro($"Falha ao finalizar conversa com {senderId}.", null, statusResult?.Error ?? "Resposta inesperada da API.");
                }
            }
            catch (ApiException apiEx) // Captura nossa exceção personalizada
            {
                MostrarErro(apiEx.Message, apiEx.InnerException, apiEx.ApiResponse);
            }
            catch (Exception ex) // Captura outras exceções (ex: da UI)
            {
                MostrarErro($"Erro inesperado ao finalizar conversa com {senderId}", ex);
            }
            finally
            {
                FinalizarCarregamento();            }
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
        private void PopularListViewEncerradas(List<ConversationStatusApi> conversas)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => PopularListViewEncerradas(conversas))); return; }
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
            lblValorSenderId.Text = string.Empty; lblValorStatus.Text = string.Empty; lblValorOpedAt.Text = string.Empty; lblValorTempoAberto.Text = string.Empty; btnFinalizarSelecionada.Enabled = false;
        }

        // Mostra detalhes da conversa selecionada
        private void MostrarDetalhesConversaSelecionada(ConversationStatusApi conv)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => MostrarDetalhesConversaSelecionada(conv))); return; }
            if (conv == null) { LimparDetalhes(); return; }
            lblValorSenderId.Text = !string.IsNullOrWhiteSpace(conv.ContactName) ? $"{conv.ContactName} ({conv.SenderId})" : conv.SenderId; lblValorStatus.Text = conv.Status; lblValorOpedAt.Text = conv.CreationDateTime.ToString("dd/MM/yyyy HH:mm:ss");
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
        private void MostrarErro(string titulo, Exception? ex = null, string? apiResponse = null)
        {
            string mensagemCompleta = titulo;

            // Pega a resposta da API se veio da ApiException
            if (ex is ApiException apiEx && !string.IsNullOrWhiteSpace(apiEx.ApiResponse))
            {
                apiResponse = apiEx.ApiResponse;
            }

            if (!string.IsNullOrWhiteSpace(apiResponse))
            {
                string erroApiDetalhe = apiResponse;
                try { using (JsonDocument document = JsonDocument.Parse(apiResponse)) { if (document.RootElement.TryGetProperty("error", out JsonElement errorElement)) { erroApiDetalhe = errorElement.GetString() ?? apiResponse; } else if (document.RootElement.TryGetProperty("message", out JsonElement msgElement)) { erroApiDetalhe = msgElement.GetString() ?? apiResponse; } } } catch { /* Ignora */ }
                mensagemCompleta += $"\n\nAPI: {erroApiDetalhe}";
            }
            if (ex != null)
            {
                var innerEx = ex;
                while (innerEx.InnerException != null) innerEx = innerEx.InnerException;
                mensagemCompleta += $"\n\nTécnico ({innerEx.GetType().Name}): {innerEx.Message}";
                // mensagemCompleta += $"\n\nStackTrace: {ex.StackTrace}";
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
    } // Fim da classe Form1
} // Fim do namespace