using System;
using System.Collections.Generic;
using System.ComponentModel; // Para BindingList
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using suporteEngenhariaUI.Exceptions; // Namespace das Exceptions
using suporteEngenhariaUI.Interfaces; // Namespace da Interface
using suporteEngenhariaUI.Models;   // Namespace dos Models

// Namespace deve corresponder à pasta onde este arquivo está (ex: UI)
namespace suporteEngenhariaUI
{
    /// <summary>
    /// Formulário principal para visualização e controle de conversas do WhatsApp via API.
    /// Utiliza DataGridView para exibição e Injeção de Dependência para o serviço da API.
    /// </summary>
    public partial class FormPrincipal : Form
    {
        // --- Constantes (para clareza) ---
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

        /// <summary>
        /// Configura propriedades iniciais e DataSources para os DataGridViews.
        /// </summary>
        private void ConfigurarGrids()
        {
            // Grid Abertas
            dgvAbertas.AutoGenerateColumns = false; // Colunas definidas no Designer
            dgvAbertas.DataSource = _conversasAbertasBindingList; // Vincula à lista
            dgvAbertas.SelectionChanged += dgv_SelectionChanged; // Associa handler comum
            dgvAbertas.CellFormatting += dgvAbertas_CellFormatting; // Formatação específica
            dgvAbertas.GotFocus += dgv_GotFocus; // Rastreia foco

            // Grid Encerradas
            dgvEncerradas.AutoGenerateColumns = false; // Colunas definidas no Designer
            dgvEncerradas.DataSource = _conversasEncerradasBindingList; // Vincula à lista
            dgvEncerradas.SelectionChanged += dgv_SelectionChanged; // Associa handler comum
            dgvEncerradas.CellFormatting += dgvEncerradas_CellFormatting; // Formatação específica
            dgvEncerradas.GotFocus += dgv_GotFocus; // Rastreia foco
        }

        // --- Evento Load do Formulário ---
        private async void FormPrincipal_Load(object sender, EventArgs e)
        {
            LimparDetalhes(); // Limpa a área de detalhes inicialmente
            // Inicia o carregamento de dados da API
            await AtualizarTodosOsDadosAsync();
        }

        // --- Método Centralizado de Atualização de Dados ---
        private async Task AtualizarTodosOsDadosAsync(bool mostrarErroGeral = true)
        {
            IniciarCarregamento();
            // Não precisamos mais da lista de errosDetalhes aqui se falharmos no primeiro erro
            Dictionary<string, ConversationStatusApi>? todosStatus = null;
            string? idAbertasSelecionado = GetSelectedConversation(dgvAbertas)?.SenderId;
            string? idEncerradasSelecionado = GetSelectedConversation(dgvEncerradas)?.SenderId;

            try // ÚNICO TRY EXTERNO
            {
                // 1. Carregar Contagens (Sem try/catch interno)
                ContagemConversasApi? contagens = await _apiService.GetCountsAsync();
                if (contagens != null) { AtualizarLabelsContagem(contagens.ContagemNovas, contagens.ContagemAbertas, contagens.ContagemEncerradas); }
                else { AtualizarLabelsContagem(-1, -1, -1); Console.WriteLine("WARN: API retornou contagens nulas."); /* Ou lançar exceção? */ }

                // 2. Carregar TODOS os Status (Sem try/catch interno)
                todosStatus = await _apiService.GetAllStatusesAsync();
                if (todosStatus == null) { Console.WriteLine("WARN: API retornou status nulo."); /* Ou lançar exceção? */ }

                // 3. Filtrar e Popular Grids (só executa se os de cima não falharam)
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

                // 4. Se chegou aqui sem exceção, não precisa mostrar erro agregado de carregamento parcial
            }
            catch (Exception ex) // CAPTURA QUALQUER ERRO das chamadas API ou processamento
            {
                // Se qualquer chamada falhar (ex: conexão), cairá aqui diretamente.
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
                // Assegura que o estado de carregamento termine e a UI seja atualizada
                FinalizarCarregamento();
                AtualizarEstadoBotoesSelecao();
            }
        }

        // --- Métodos de Atualização da UI ---

        /// <summary>
        /// Helper genérico para popular um DataGridView ligado a uma BindingList,
        /// limpando a lista, adicionando novos itens e tentando restaurar a seleção anterior.
        /// </summary>
        private void PopularGrid(BindingList<ConversationStatusApi> bindingList, DataGridView dgv, List<ConversationStatusApi> novasConversas, string? selectedSenderId)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => PopularGrid(bindingList, dgv, novasConversas, selectedSenderId))); return; }

            // Desvincular temporariamente pode melhorar a performance em alguns cenários, mas BindingList geralmente lida bem.
            // dgv.DataSource = null;
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

        /// <summary>
        /// Tenta encontrar e selecionar uma linha no DataGridView baseado no SenderId.
        /// Limpa a seleção se o ID não for encontrado ou for nulo/vazio.
        /// </summary>
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
                // Tenta rolar para a linha selecionada
                // Garante que o índice está dentro dos limites antes de definir FirstDisplayedScrollingRowIndex
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

        /// <summary>
        /// Exibe os detalhes da conversa fornecida na área de detalhes do formulário.
        /// Limpa os detalhes se a conversa for nula.
        /// </summary>
        private void MostrarDetalhesConversaSelecionada(ConversationStatusApi? conv)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => MostrarDetalhesConversaSelecionada(conv))); return; }
            if (conv == null) { LimparDetalhes(); return; }

            lblValorSenderId.Text = conv.DisplayName; // Usa a propriedade calculada
            lblValorStatus.Text = conv.Status ?? "N/D";
            lblValorOpedAt.Text = conv.CreationDateTime.ToString("dd/MM/yyyy HH:mm:ss"); // Formato longo para detalhes

            // Calcula e formata a duração ou tempo em aberto
            TimeSpan duracao; string tempoFormatado;
            if (conv.Status != null && conv.Status.Equals(StatusConstants.Open, StringComparison.OrdinalIgnoreCase))
            {
                // Conversa aberta: Calcula tempo desde a criação até agora
                duracao = DateTime.Now - conv.CreationDateTime;
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

        /// <summary>
        /// Formata um TimeSpan em uma string legível (ex: "1d 2h 30m", "< 1m").
        /// </summary>
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

        /// <summary>
        /// Exibe uma mensagem de erro formatada em uma MessageBox e no Console.
        /// Inclui detalhes da API e da exceção interna, se disponíveis.
        /// </summary>
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
                // --- CORREÇÃO AQUI ---
                catch (JsonException jsonEx) // Primeiro pega o erro específico de JSON inválido
                {
                    // Em vez de ignorar silenciosamente, é melhor logar que o parse falhou
                    Console.WriteLine($"WARN: Não foi possível analisar a resposta da API como JSON no MostrarErro: {jsonEx.Message}. Resposta original será exibida.");
                    // Mantém erroApiFormatado como a string original da API.
                }
                catch (Exception pe) // Depois pega qualquer outro erro inesperado durante o parse/processamento
                {
                    Console.WriteLine($"Erro inesperado ao processar detalhes da API no MostrarErro: {pe.Message}");
                    // Mantém erroApiFormatado como a string original da API.
                }
                // --- FIM DA CORREÇÃO ---

                mensagemCompleta += $"\n\nDetalhe API: {erroApiFormatado}"; // Usa o valor formatado ou o original
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

        /// <summary>
        /// Evento disparado quando um dos grids ganha foco. Armazena qual grid está ativo.
        /// </summary>
        private void dgv_GotFocus(object sender, EventArgs e) { _lastFocusedGrid = sender as DataGridView; }

        /// <summary>
        /// Obtém o objeto ConversationStatusApi da linha atualmente selecionada no DataGridView fornecido.
        /// Retorna null se nenhuma linha estiver selecionada ou se o objeto vinculado for inválido.
        /// </summary>
        private ConversationStatusApi? GetSelectedConversation(DataGridView dgv)
        {
            if (dgv != null && dgv.SelectedRows.Count > 0 && dgv.SelectedRows[0].DataBoundItem is ConversationStatusApi conv)
            {
                return conv;
            }
            return null;
        }

        /// <summary>
        /// Handler de evento UNIFICADO para SelectionChanged de ambos os DataGridViews.
        /// Limpa a seleção do outro grid e atualiza os detalhes e botões.
        /// </summary>
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

        /// <summary>
        /// Atualiza o estado do botão Finalizar e a área de detalhes com base na seleção atual
        /// do último grid que recebeu foco.
        /// </summary>
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
                    TimeSpan tempoDecorrido = DateTime.Now - conv.CreationDateTime;
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
            // Formata a coluna "Duração"
            // IMPORTANTE: Use o NOME da coluna definido no Designer
            if (e.RowIndex >= 0 && dgvEncerradas.Columns[e.ColumnIndex].Name == "colEncerradasDuracao")
            {
                if (dgvEncerradas.Rows[e.RowIndex].DataBoundItem is ConversationStatusApi conv && conv.DuracaoConversa.HasValue)
                {
                    // Usa a propriedade calculada DuracaoConversa e formata
                    e.Value = FormatarTempoDecorrido(conv.DuracaoConversa.Value);
                    e.FormattingApplied = true;
                }
                else
                {
                    e.Value = "N/A"; // Mostra N/A se não houver duração calculada
                    e.FormattingApplied = true;
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