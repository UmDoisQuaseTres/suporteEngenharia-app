using System;
using System.Collections.Generic;
using System.ComponentModel; // Para BindingList
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using suporteEngenhariaUI.Exceptions; 
using suporteEngenhariaUI.Interfaces; 
using suporteEngenhariaUI.Models;


namespace suporteEngenhariaUI
{

    public partial class FormPrincipal : Form
    {
        private static class StatusConstants
        {
            public const string Open = "open";
            public const string Closed = "closed";
            public const string AlreadyClosed = "already_closed";
            public const string NotFound = "not_found";
            public const string Error = "error";
        }

        // --- Serviço Injetado ---
        private readonly IWhatsAppApiService _apiService;

        // --- Fontes de Dados para Grids ---
        private BindingList<ConversationStatusApi> _conversasAbertasBindingList;
        private BindingList<ConversationStatusApi> _conversasEncerradasBindingList;

        // --- Controle de Foco para Seleção ---
        private DataGridView? _lastFocusedGrid = null;

        // --- Construtor com Injeção de Dependência ---
        public FormPrincipal(IWhatsAppApiService apiService)
        {
            InitializeComponent();

            // Armazena a instância injetada do serviço
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));

            // Inicializa as listas que serão vinculadas aos grids
            _conversasAbertasBindingList = new BindingList<ConversationStatusApi>();
            _conversasEncerradasBindingList = new BindingList<ConversationStatusApi>();

            // Configura os DataGridViews (DataSource, eventos, etc.)
            ConfigurarGrids();
        }
        private void ConfigurarGrids()
        {
            // Grid Abertas
            dgvAbertas.AutoGenerateColumns = false;
            dgvAbertas.DataSource = _conversasAbertasBindingList;
            dgvAbertas.SelectionChanged += dgv_SelectionChanged;
            dgvAbertas.CellFormatting += dgvAbertas_CellFormatting;
            dgvAbertas.GotFocus += dgv_GotFocus;
            colAbertasCliente.DataPropertyName = "SenderId";
            colAbertasInicio.DataPropertyName = "CreationTimestamp";
            colAbertasStatus.DataPropertyName = "Status";


            // Grid Encerradas
            // --- Grid Encerradas (Usando CellFormatting para Datas) ---
            dgvEncerradas.AutoGenerateColumns = false;
            dgvEncerradas.DataSource = _conversasEncerradasBindingList;
            dgvEncerradas.SelectionChanged += dgv_SelectionChanged;
            dgvEncerradas.CellFormatting += dgvEncerradas_CellFormatting; // Já estava
            dgvEncerradas.GotFocus += dgv_GotFocus; // Já estava

            // Verifique os nomes das variáveis das colunas no Designer!
            colEncerradasCliente.DataPropertyName = "DisplayName";     // OK
            colEncerradasStatus.DataPropertyName = "Status";          // OK

            // Definir como null pois CellFormatting vai cuidar da exibição
            colEncerradasInicio.DataPropertyName = null;
            colEncerradasFim.DataPropertyName = null;
            colEncerradasDuracao.DataPropertyName = null;
        }

        // --- Evento Load do Formulário ---
        private async void FormPrincipal_Load(object sender, EventArgs e)
        {
            LimparDetalhes(); 
            await AtualizarTodosOsDadosAsync();
        }

        // --- Método Centralizado de Atualização de Dados ---
        private async Task AtualizarTodosOsDadosAsync(bool mostrarErroGeral = true)
        {
            IniciarCarregamento();
            Dictionary<string, ConversationStatusApi>? todosStatus = null;
            string? idAbertasSelecionado = GetSelectedConversation(dgvAbertas)?.SenderId;
            string? idEncerradasSelecionado = GetSelectedConversation(dgvEncerradas)?.SenderId;

            try
            {
                ContagemConversasApi? contagens = await _apiService.GetCountsAsync();
                if (contagens != null) { AtualizarLabelsContagem(contagens.ContagemNovas, contagens.ContagemAbertas, contagens.ContagemEncerradas); }
                else { AtualizarLabelsContagem(-1, -1, -1); Console.WriteLine("WARN: API retornou contagens nulas."); /* Ou lançar exceção? */ }
                todosStatus = await _apiService.GetAllStatusesAsync();
                if (todosStatus == null) { Console.WriteLine("WARN: API retornou status nulo."); /* Ou lançar exceção? */ }

                var conversasAbertas = todosStatus?.Values
                    .Where(conv => conv.Status != null && conv.Status.Equals(StatusConstants.Open, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(conv => conv.CreationTimestamp)
                    .ToList() ?? new List<ConversationStatusApi>();
                PopularGrid(_conversasAbertasBindingList, dgvAbertas, conversasAbertas, idAbertasSelecionado);

                var conversasEncerradas = todosStatus?.Values
                    .Where(conv => conv.Status != null && conv.Status.Equals(StatusConstants.Closed, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(conv => conv.ClosedTimestamp ?? conv.CreationTimestamp)
                    .ToList() ?? new List<ConversationStatusApi>();
                PopularGrid(_conversasEncerradasBindingList, dgvEncerradas, conversasEncerradas, idEncerradasSelecionado);

            }
            catch (Exception ex) 
            {
                if (mostrarErroGeral)
                {
                    // Mostra o erro específico que ocorreu (ex: HttpRequestException)
                    MostrarErro("Falha ao carregar dados da API.", ex);
                }
                else { Console.WriteLine($"ERRO GERAL (não exibido UI): {ex.GetType().Name} - {ex.Message}"); }

                // Limpa os grids e labels para indicar falha completa
                AtualizarLabelsContagem(-1, -1, -1);
                PopularGrid(_conversasAbertasBindingList, dgvAbertas, new List<ConversationStatusApi>(), null);
                PopularGrid(_conversasEncerradasBindingList, dgvEncerradas, new List<ConversationStatusApi>(), null);
            }
            finally // SEMPRE será executado
            {
                FinalizarCarregamento(); // Restaura UI ao estado normal
                AtualizarEstadoBotoesSelecao(); // Atualiza botões/detalhes
            }
        }

        // --- Métodos Auxiliares de Carregamento UI ---
        private void IniciarCarregamento()
        {
            if (this.InvokeRequired) { this.Invoke(new Action(IniciarCarregamento)); return; }
            this.Cursor = Cursors.WaitCursor;
            btnFinalizarSelecionada.Enabled = false;
            btnAtualizarEncerradas.Enabled = false;
            // Tenta encontrar o botão de atualizar tudo pelo nome padrão ou fallback
            var btnAtualizarTudo = this.Controls.Find("btnAtualizarTudo", true).FirstOrDefault() ??
                                   this.Controls.Find("button1", true).FirstOrDefault();
            if (btnAtualizarTudo is Button btn) btn.Enabled = false;

            btnAtualizarAbertas.Enabled = false;
            toolStripStatusLabelInfo.Text = "Carregando dados...";
            toolStripProgressBar.Visible = true;
            toolStripProgressBar.Style = ProgressBarStyle.Marquee;
        }

        private void FinalizarCarregamento()
        {
            if (this.InvokeRequired) { this.Invoke(new Action(FinalizarCarregamento)); return; }
            this.Cursor = Cursors.Default;
            btnAtualizarEncerradas.Enabled = true;
            var btnAtualizarTudo = this.Controls.Find("btnAtualizarTudo", true).FirstOrDefault() ??
                                  this.Controls.Find("button1", true).FirstOrDefault();
            if (btnAtualizarTudo is Button btn) btn.Enabled = true;
            btnAtualizarAbertas.Enabled = true;
            // O estado do botão Finalizar é tratado por AtualizarEstadoBotoesSelecao
            toolStripStatusLabelInfo.Text = $"Pronto. Dados atualizados às {DateTime.Now:HH:mm:ss}";
            toolStripProgressBar.Visible = false;
        }

        // --- Ação API: Finalizar Conversa ---
        private async Task FinalizarConversaApiAsync(ConversationStatusApi conversa)
        {
            if (conversa == null) return;
            string senderId = conversa.SenderId; // Usa o ID do objeto passado

            IniciarCarregamento();
            try
            {
                CloseStatusApi? statusResult = await _apiService.CloseConversationAsync(senderId);
                if (statusResult == null) { MostrarErro($"Falha ao finalizar {senderId}. Resposta nula do serviço."); return; }

                // Analisa o resultado retornado pela API
                switch (statusResult.Status?.ToLowerInvariant())
                {
                    case StatusConstants.Closed:
                        MessageBox.Show($"Conversa com {conversa.DisplayName} ({senderId}) finalizada com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await AtualizarTodosOsDadosAsync(false); // Recarrega sem msg erro geral
                        break;
                    case StatusConstants.AlreadyClosed:
                        MessageBox.Show($"Conversa com {conversa.DisplayName} ({senderId}) já estava finalizada.", "Informação", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await AtualizarTodosOsDadosAsync(false);
                        break;
                    case StatusConstants.NotFound:
                        MessageBox.Show($"Conversa com {senderId} não encontrada na API.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        await AtualizarTodosOsDadosAsync(false);
                        break;
                    case StatusConstants.Error:
                        MostrarErro($"Falha ao finalizar conversa com {senderId}.", apiResponse: statusResult.Error ?? "API retornou status 'error'.");
                        break;
                    default: // Status inesperado ou nulo
                        MostrarErro($"Falha ao finalizar conversa com {senderId}.", apiResponse: statusResult.Error ?? $"Status inesperado '{statusResult.Status ?? "null"}' da API.");
                        break;
                }
            }
            catch (ApiException apiEx) { MostrarErro($"Erro da API ao finalizar {senderId}: {apiEx.Message}", apiEx, apiEx.ApiResponse); }
            catch (Exception ex) { MostrarErro($"Erro inesperado ao finalizar {senderId}", ex); }
            finally
            {
                FinalizarCarregamento();
                AtualizarEstadoBotoesSelecao();
            }
        }
        private void PopularGrid(BindingList<ConversationStatusApi> bindingList, DataGridView dgv, List<ConversationStatusApi> novasConversas, string? selectedSenderId)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => PopularGrid(bindingList, dgv, novasConversas, selectedSenderId))); return; }

            try
            {
                bindingList.RaiseListChangedEvents = false; // Desabilita notificações durante a carga em massa
                bindingList.Clear();
                if (novasConversas != null)
                {
                    foreach (var conv in novasConversas) // A lista já deve vir ordenada
                    {
                        bindingList.Add(conv);
                    }
                }
            }
            finally
            {
                bindingList.RaiseListChangedEvents = true; // Reabilita notificações
                bindingList.ResetBindings(); // Força a atualização do grid
                                             // dgv.DataSource = bindingList; // Reatribui se desvinculou antes
                RestaurarSelecaoGrid(dgv, selectedSenderId); // Tenta restaurar a seleção
            }
        }
        private void RestaurarSelecaoGrid(DataGridView dgv, string? selectedSenderId)
        {
            if (string.IsNullOrEmpty(selectedSenderId))
            {
                if (dgv.SelectedRows.Count > 0) dgv.ClearSelection();
                return;
            }

            DataGridViewRow? rowToSelect = null;
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.DataBoundItem is ConversationStatusApi conv && conv.SenderId == selectedSenderId)
                {
                    rowToSelect = row;
                    break; // Para após encontrar
                }
            }

            if (rowToSelect != null)
            {
                // Garante que apenas a linha correta seja selecionada
                if (!rowToSelect.Selected)
                {
                    dgv.ClearSelection();
                    rowToSelect.Selected = true;
                }
               
                if (dgv.RowCount > 0 && rowToSelect.Index >= 0 && rowToSelect.Index < dgv.RowCount)
                {
                    // Centraliza um pouco melhor se possível
                    int desiredFirstRow = Math.Max(0, rowToSelect.Index - dgv.DisplayedRowCount(false) / 2);
                    // Garante que não ultrapasse o limite inferior
                    desiredFirstRow = Math.Min(desiredFirstRow, dgv.RowCount - 1);
                    if (desiredFirstRow >= 0)
                    {
                        dgv.FirstDisplayedScrollingRowIndex = desiredFirstRow;
                    }
                }
            }
            else
            {
                // Se o item selecionado anteriormente não existe mais, limpa a seleção
                if (dgv.SelectedRows.Count > 0) dgv.ClearSelection();
            }
        }

        private void AtualizarLabelsContagem(int novas, int abertas, int encerradas)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => AtualizarLabelsContagem(novas, abertas, encerradas))); return; }
            lblContagemNovas.Text = (novas < 0) ? "Novas: Erro" : $"Novas: {novas}";
            lblContagemAbertas.Text = (abertas < 0) ? "Em Aberto: Erro" : $"Em Aberto: {abertas}";
            lblContagemEncerradas.Text = (encerradas < 0) ? "Encerradas: Erro" : $"Encerradas: {encerradas}";
        }

        private void LimparDetalhes()
        {
            if (this.InvokeRequired) { this.Invoke(new Action(LimparDetalhes)); return; }
            lblValorSenderId.Text = string.Empty;
            lblValorStatus.Text = string.Empty;
            lblValorOpedAt.Text = string.Empty;
            lblValorTempoAberto.Text = string.Empty;
        }
        private void MostrarDetalhesConversaSelecionada(ConversationStatusApi? conv)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => MostrarDetalhesConversaSelecionada(conv))); return; }
            if (conv == null) { LimparDetalhes(); return; }

            lblValorSenderId.Text = conv.DisplayName; // Usa a propriedade calculada
            lblValorStatus.Text = conv.Status ?? "N/D";
            lblValorOpedAt.Text = conv.CreationTimestamp.ToString("dd/MM/yyyy HH:mm:ss"); // Formato longo para detalhes

            // Calcula e formata a duração ou tempo em aberto
            TimeSpan duracao; string tempoFormatado;
            if (conv.Status != null && conv.Status.Equals(StatusConstants.Open, StringComparison.OrdinalIgnoreCase))
            {
                // Conversa aberta: Calcula tempo desde a criação até agora
                duracao = DateTime.Now - conv.CreationTimestamp;
                tempoFormatado = FormatarTempoDecorrido(duracao);
                lblValorTempoAberto.Text = tempoFormatado; // Usa o label padrão
            }
            else if (conv.DuracaoConversa.HasValue)
            {
                // Conversa fechada: Usa a duração pré-calculada
                duracao = conv.DuracaoConversa.Value;
                tempoFormatado = FormatarTempoDecorrido(duracao);
                lblValorTempoAberto.Text = tempoFormatado + " (Total)"; // Adiciona "(Total)" para clareza
            }
            else { lblValorTempoAberto.Text = "N/A"; } // Caso inesperado
        }
        private string FormatarTempoDecorrido(TimeSpan duracao)
        {
            if (duracao.TotalMinutes < 1) return "< 1m";
            var sb = new StringBuilder();
            if (duracao.Days > 0) sb.Append($"{duracao.Days}d ");
            if (duracao.Hours > 0) sb.Append($"{duracao.Hours}h ");
            if (duracao.Minutes > 0) sb.Append($"{duracao.Minutes}m");
            // Se por acaso ficou vazio (ex: exatamente 0 minutos, mas > 0 segundos), retorna < 1m
            if (sb.Length == 0) return "< 1m";
            return sb.ToString().TrimEnd();
        }

        private void MostrarErro(string titulo, Exception? ex = null, string? apiResponse = null)
        {
            string mensagemCompleta = titulo;
            string? erroApiDetalhe = apiResponse;

            // Pega a resposta da API se veio da ApiException
            if (ex is ApiException apiEx && !string.IsNullOrWhiteSpace(apiEx.ApiResponse))
            {
                erroApiDetalhe = apiEx.ApiResponse;
            }

            if (!string.IsNullOrWhiteSpace(erroApiDetalhe))
            {
                string erroApiFormatado = erroApiDetalhe; // Default é a string crua
                try
                {
                    // Tenta parsear como JSON para extrair 'error' ou 'message'
                    using (JsonDocument document = JsonDocument.Parse(erroApiDetalhe))
                    {
                        if (document.RootElement.TryGetProperty("error", out JsonElement errorElement) && errorElement.ValueKind == JsonValueKind.String)
                        {
                            erroApiFormatado = errorElement.GetString() ?? erroApiDetalhe;
                        }
                        else if (document.RootElement.TryGetProperty("message", out JsonElement msgElement) && msgElement.ValueKind == JsonValueKind.String)
                        {
                            erroApiFormatado = msgElement.GetString() ?? erroApiDetalhe;
                        }
                        // Se não encontrar 'error' ou 'message', mantém a string original (erroApiFormatado)
                    }
                }
                catch (JsonException jsonEx) 
                {
                    Console.WriteLine($"WARN: Não foi possível analisar a resposta da API como JSON no MostrarErro: {jsonEx.Message}. Resposta original será exibida.");
                }
                catch (Exception pe) 
                {
                    Console.WriteLine($"Erro inesperado ao processar detalhes da API no MostrarErro: {pe.Message}");
                    
                }

                mensagemCompleta += $"\n\nDetalhe API: {erroApiFormatado}"; 
            }

            // Adiciona detalhes da exceção principal (InnerMost)
            if (ex != null)
            {
                var innerEx = ex;
                while (innerEx.InnerException != null)
                {
                    innerEx = innerEx.InnerException;
                }
                mensagemCompleta += $"\n\nErro Técnico ({innerEx.GetType().Name}): {innerEx.Message}";
            }

            // Garante que a MessageBox seja exibida na thread da UI
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => MessageBox.Show(this, mensagemCompleta, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error)));
            }
            else
            {
                MessageBox.Show(this, mensagemCompleta, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Loga o erro completo no console/log
            Console.WriteLine($"ERRO Exibido: {mensagemCompleta.Replace("\n", " | ")}");
        }


        // --- Event Handlers para DataGridView ---

        private void dgv_GotFocus(object sender, EventArgs e) { _lastFocusedGrid = sender as DataGridView; }

        private ConversationStatusApi? GetSelectedConversation(DataGridView dgv)
        {
            if (dgv != null && dgv.SelectedRows.Count > 0 && dgv.SelectedRows[0].DataBoundItem is ConversationStatusApi conv)
            {
                return conv;
            }
            return null;
        }

        private void dgv_SelectionChanged(object? sender, EventArgs e)
        {
            var currentGrid = sender as DataGridView;
            // Só processa se o grid que disparou o evento estiver realmente focado (evita reprocessamento ao limpar seleção do outro)
            if (currentGrid == null || !currentGrid.Focused) return;

            // Determina qual é o outro grid
            var otherGrid = (currentGrid == dgvAbertas) ? dgvEncerradas : dgvAbertas;

            // Limpa a seleção no outro grid APENAS se ele tiver alguma seleção
            if (otherGrid.SelectedRows.Count > 0)
            {
                otherGrid.ClearSelection();
            }

            // Atualiza o estado dos botões e a área de detalhes com base na seleção atual do grid ativo
            AtualizarEstadoBotoesSelecao();
        }

        private void AtualizarEstadoBotoesSelecao()
        {
            // Invoca na thread da UI se necessário
            if (this.InvokeRequired) { this.Invoke(new Action(AtualizarEstadoBotoesSelecao)); return; }

            ConversationStatusApi? selecionada = null;
            bool isAberta = false;

            // Verifica qual grid teve foco por último (ou se algum tem seleção)
            var gridAtivo = _lastFocusedGrid;
            if (gridAtivo == null)
            { // Tenta pegar algum selecionado se nenhum teve foco ainda
                gridAtivo = (GetSelectedConversation(dgvAbertas) != null) ? dgvAbertas :
                            (GetSelectedConversation(dgvEncerradas) != null) ? dgvEncerradas : null;
            }

            if (gridAtivo != null)
            {
                selecionada = GetSelectedConversation(gridAtivo);
                isAberta = (gridAtivo == dgvAbertas);
            }

            // Habilita o botão Finalizar APENAS se uma conversa ABERTA estiver selecionada
            btnFinalizarSelecionada.Enabled = (selecionada != null && isAberta && selecionada.Status?.Equals(StatusConstants.Open, StringComparison.OrdinalIgnoreCase) == true);

            // Mostra os detalhes da conversa selecionada (ou limpa se nenhuma)
            MostrarDetalhesConversaSelecionada(selecionada);
        }


        // --- Eventos de Formatação de Células ---
        private void dgvAbertas_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Formata a coluna "Tempo em aberto"
            // IMPORTANTE: Use o NOME da coluna definido no Designer
            if (e.RowIndex >= 0 && dgvAbertas.Columns[e.ColumnIndex].Name == "colAbertasTempo")
            {
                if (dgvAbertas.Rows[e.RowIndex].DataBoundItem is ConversationStatusApi conv)
                {
                    // Calcula a diferença entre agora e a data de criação
                    TimeSpan tempoDecorrido = DateTime.Now - conv.CreationTimestamp;
                    e.Value = FormatarTempoDecorrido(tempoDecorrido); // Formata o TimeSpan
                    e.FormattingApplied = true; // Informa ao grid que a formatação foi feita
                }
                else
                {
                    e.Value = "N/A"; // Valor padrão se o objeto não for encontrado
                    e.FormattingApplied = true;
                }
            }
        }

        private void dgvEncerradas_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Certifique-se que os NOMES das colunas ("colEncerradasInicio", etc.) estão corretos!
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0) // Boa prática adicionar verificação de ColumnIndex
            {
                // Tenta obter o objeto ConversationStatusApi da linha atual
                if (dgvEncerradas.Rows[e.RowIndex].DataBoundItem is ConversationStatusApi conv)
                {
                    // Formata a coluna "Aberto em"
                    if (dgvEncerradas.Columns[e.ColumnIndex].Name == "colEncerradasInicio") // <-- VERIFIQUE O NOME!
                    {
                        // Usa a propriedade calculada DateTime
                        e.Value = conv.CreationTimestamp.ToString("dd/MM/yyyy HH:mm"); // Ou outro formato desejado
                        e.FormattingApplied = true;
                    }
                    // Formata a coluna "Data fechamento"
                    else if (dgvEncerradas.Columns[e.ColumnIndex].Name == "colEncerradasFim") // <-- VERIFIQUE O NOME!
                    {
                        // Usa a propriedade calculada DateTime? (nullable)
                        if (conv.ClosedTimestamp.HasValue)
                        {
                            e.Value = conv.ClosedTimestamp.Value.ToString("dd/MM/yyyy HH:mm"); // Ou outro formato
                        }
                        else
                        {
                            e.Value = "N/A"; // Ou string vazia se preferir
                        }
                        e.FormattingApplied = true;
                    }
                    // Formata a coluna "Duração" (como já estava)
                    else if (dgvEncerradas.Columns[e.ColumnIndex].Name == "colEncerradasDuracao") // <-- VERIFIQUE O NOME!
                    {
                        if (conv.DuracaoConversa.HasValue)
                        {
                            e.Value = FormatarTempoDecorrido(conv.DuracaoConversa.Value);
                        }
                        else
                        {
                            e.Value = "N/A";
                        }
                        e.FormattingApplied = true;
                    }
                }
                else if (e.Value == null) // Se não conseguiu pegar o objeto, define como N/A para evitar erro
                {
                    // Para evitar erros se DataBoundItem for null por algum motivo
                    if (dgvEncerradas.Columns[e.ColumnIndex].Name == "colEncerradasInicio" ||
                        dgvEncerradas.Columns[e.ColumnIndex].Name == "colEncerradasFim" ||
                        dgvEncerradas.Columns[e.ColumnIndex].Name == "colEncerradasDuracao")
                    {
                        e.Value = "N/A";
                        e.FormattingApplied = true;
                    }
                }
            }
        }


        // --- Event Handlers Botões ---
        private async void btnFinalizarSelecionada_Click(object sender, EventArgs e)
        {
            // Pega a conversa selecionada SOMENTE do grid de abertas
            ConversationStatusApi? conv = GetSelectedConversation(dgvAbertas);
            if (conv != null)
            {
                // Confirma com o usuário
                DialogResult confirm = MessageBox.Show($"Tem certeza que deseja finalizar a conversa com {conv.DisplayName}?",
                                                       "Confirmar Finalização", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    await FinalizarConversaApiAsync(conv); // Chama a ação passando o objeto
                }
            }
            else { MessageBox.Show("Nenhuma conversa aberta selecionada para finalizar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        }

        // Assume que o botão Atualizar Tudo foi renomeado para btnAtualizarTudo ou é button1
        private async void btnAtualizarTudo_Click(object sender, EventArgs e)
        {
            await AtualizarTodosOsDadosAsync();
        }

        private async void btnAtualizarEncerradas_Click(object sender, EventArgs e)
        {
            IniciarCarregamento(); try
            {
                var todos = await _apiService.GetAllStatusesAsync();
                var data = todos?.Values.Where(c => c.Status?.Equals(StatusConstants.Closed, StringComparison.OrdinalIgnoreCase) ?? false)
                                 .OrderByDescending(c => c.ClosedTimestamp ?? c.CreationTimestamp).ToList() ?? new List<ConversationStatusApi>();
                // Popula o grid específico e tenta manter a seleção
                PopularGrid(_conversasEncerradasBindingList, dgvEncerradas, data, GetSelectedConversation(dgvEncerradas)?.SenderId);
                // Opcional: Atualizar contagens também para consistência
                // var contagens = await _apiService.GetCountsAsync(); AtualizarLabelsContagem(...);
            }
            catch (Exception ex) { MostrarErro("Erro ao atualizar lista de encerradas", ex); }
            finally { FinalizarCarregamento(); AtualizarEstadoBotoesSelecao(); }
        }

        private async void btnAtualizarAbertas_Click(object sender, EventArgs e)
        {
            IniciarCarregamento(); try
            {
                var todos = await _apiService.GetAllStatusesAsync();
                var data = todos?.Values.Where(c => c.Status?.Equals(StatusConstants.Open, StringComparison.OrdinalIgnoreCase) ?? false)
                                 .OrderByDescending(c => c.CreationTimestamp).ToList() ?? new List<ConversationStatusApi>();
                // Popula o grid específico e tenta manter a seleção
                PopularGrid(_conversasAbertasBindingList, dgvAbertas, data, GetSelectedConversation(dgvAbertas)?.SenderId);
                // Opcional: Atualizar contagens
            }
            catch (Exception ex) { MostrarErro("Erro ao atualizar lista de abertas", ex); }
            finally { FinalizarCarregamento(); AtualizarEstadoBotoesSelecao(); }
        }

    } // Fim da classe FormPrincipal
} // Fim do namespace