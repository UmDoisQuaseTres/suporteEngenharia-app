namespace suporteEngenhariaUI
{
    partial class FormPrincipal
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ColumnHeader colClientName;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPrincipal));
            splitContainerFinalizar = new SplitContainer();
            listViewAbertasParaFinalizar = new ListView();
            colOpenAt = new ColumnHeader();
            colStatus = new ColumnHeader();
            btnAtualizarAbertas = new Button();
            btnFinalizarSelecionada = new Button();
            grpDetalhesMensagem = new GroupBox();
            tableLayoutPanelDetalhes = new TableLayoutPanel();
            lblValorTempoAberto = new Label();
            lblTimeOpened = new Label();
            lblValorOpedAt = new Label();
            lblValorStatus = new Label();
            lblDescSenderId = new Label();
            lblDescStatus = new Label();
            lblOpenedAt = new Label();
            lblValorSenderId = new Label();
            tabControlEngenharia = new TabControl();
            tabPageVisaoGeral = new TabPage();
            btnAtualizar = new Button();
            grpResumoEncerradas = new GroupBox();
            lblContagemEncerradas = new Label();
            grpResumoAbertas = new GroupBox();
            lblContagemAbertas = new Label();
            grpResumoNovas = new GroupBox();
            lblContagemNovas = new Label();
            tabPageFinalizar = new TabPage();
            tabPageEncerradas = new TabPage();
            btnAtualizarEncerradas = new Button();
            listViewEncerradas = new ListView();
            colClientId = new ColumnHeader();
            colCreatedAt = new ColumnHeader();
            colEncStatus = new ColumnHeader();
            colDataFechado = new ColumnHeader();
            colTempoDecorrido = new ColumnHeader();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabelInfo = new ToolStripStatusLabel();
            toolStripProgressBar = new ToolStripProgressBar();
            colClientName = new ColumnHeader();
            ((System.ComponentModel.ISupportInitialize)splitContainerFinalizar).BeginInit();
            splitContainerFinalizar.Panel1.SuspendLayout();
            splitContainerFinalizar.Panel2.SuspendLayout();
            splitContainerFinalizar.SuspendLayout();
            grpDetalhesMensagem.SuspendLayout();
            tableLayoutPanelDetalhes.SuspendLayout();
            tabControlEngenharia.SuspendLayout();
            tabPageVisaoGeral.SuspendLayout();
            grpResumoEncerradas.SuspendLayout();
            grpResumoAbertas.SuspendLayout();
            grpResumoNovas.SuspendLayout();
            tabPageFinalizar.SuspendLayout();
            tabPageEncerradas.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // colClientName
            // 
            colClientName.Tag = "ID Remetente";
            resources.ApplyResources(colClientName, "colClientName");
            // 
            // splitContainerFinalizar
            // 
            resources.ApplyResources(splitContainerFinalizar, "splitContainerFinalizar");
            splitContainerFinalizar.Name = "splitContainerFinalizar";
            // 
            // splitContainerFinalizar.Panel1
            // 
            resources.ApplyResources(splitContainerFinalizar.Panel1, "splitContainerFinalizar.Panel1");
            splitContainerFinalizar.Panel1.Controls.Add(listViewAbertasParaFinalizar);
            // 
            // splitContainerFinalizar.Panel2
            // 
            resources.ApplyResources(splitContainerFinalizar.Panel2, "splitContainerFinalizar.Panel2");
            splitContainerFinalizar.Panel2.Controls.Add(btnAtualizarAbertas);
            splitContainerFinalizar.Panel2.Controls.Add(btnFinalizarSelecionada);
            splitContainerFinalizar.Panel2.Controls.Add(grpDetalhesMensagem);
            // 
            // listViewAbertasParaFinalizar
            // 
            resources.ApplyResources(listViewAbertasParaFinalizar, "listViewAbertasParaFinalizar");
            listViewAbertasParaFinalizar.Columns.AddRange(new ColumnHeader[] { colClientName, colOpenAt, colStatus });
            listViewAbertasParaFinalizar.FullRowSelect = true;
            listViewAbertasParaFinalizar.GridLines = true;
            listViewAbertasParaFinalizar.Name = "listViewAbertasParaFinalizar";
            listViewAbertasParaFinalizar.UseCompatibleStateImageBehavior = false;
            listViewAbertasParaFinalizar.View = View.Details;
            listViewAbertasParaFinalizar.SelectedIndexChanged += listViewAbertasParaFinalizar_SelectedIndexChanged;
            // 
            // colOpenAt
            // 
            colOpenAt.Tag = "Data de inicio";
            resources.ApplyResources(colOpenAt, "colOpenAt");
            // 
            // colStatus
            // 
            colStatus.Tag = "Status";
            resources.ApplyResources(colStatus, "colStatus");
            // 
            // btnAtualizarAbertas
            // 
            resources.ApplyResources(btnAtualizarAbertas, "btnAtualizarAbertas");
            btnAtualizarAbertas.Name = "btnAtualizarAbertas";
            btnAtualizarAbertas.UseVisualStyleBackColor = true;
            btnAtualizarAbertas.Click += btnAtualizarAbertas_Click;
            // 
            // btnFinalizarSelecionada
            // 
            resources.ApplyResources(btnFinalizarSelecionada, "btnFinalizarSelecionada");
            btnFinalizarSelecionada.Name = "btnFinalizarSelecionada";
            btnFinalizarSelecionada.UseVisualStyleBackColor = true;
            btnFinalizarSelecionada.Click += btnFinalizarSelecionada_Click;
            // 
            // grpDetalhesMensagem
            // 
            resources.ApplyResources(grpDetalhesMensagem, "grpDetalhesMensagem");
            grpDetalhesMensagem.Controls.Add(tableLayoutPanelDetalhes);
            grpDetalhesMensagem.Name = "grpDetalhesMensagem";
            grpDetalhesMensagem.TabStop = false;
            // 
            // tableLayoutPanelDetalhes
            // 
            resources.ApplyResources(tableLayoutPanelDetalhes, "tableLayoutPanelDetalhes");
            tableLayoutPanelDetalhes.Controls.Add(lblValorTempoAberto, 1, 3);
            tableLayoutPanelDetalhes.Controls.Add(lblTimeOpened, 0, 3);
            tableLayoutPanelDetalhes.Controls.Add(lblValorOpedAt, 1, 2);
            tableLayoutPanelDetalhes.Controls.Add(lblValorStatus, 1, 1);
            tableLayoutPanelDetalhes.Controls.Add(lblDescSenderId, 0, 0);
            tableLayoutPanelDetalhes.Controls.Add(lblDescStatus, 0, 1);
            tableLayoutPanelDetalhes.Controls.Add(lblOpenedAt, 0, 2);
            tableLayoutPanelDetalhes.Controls.Add(lblValorSenderId, 1, 0);
            tableLayoutPanelDetalhes.Name = "tableLayoutPanelDetalhes";
            // 
            // lblValorTempoAberto
            // 
            resources.ApplyResources(lblValorTempoAberto, "lblValorTempoAberto");
            lblValorTempoAberto.Name = "lblValorTempoAberto";
            // 
            // lblTimeOpened
            // 
            resources.ApplyResources(lblTimeOpened, "lblTimeOpened");
            lblTimeOpened.Name = "lblTimeOpened";
            // 
            // lblValorOpedAt
            // 
            resources.ApplyResources(lblValorOpedAt, "lblValorOpedAt");
            lblValorOpedAt.Name = "lblValorOpedAt";
            // 
            // lblValorStatus
            // 
            resources.ApplyResources(lblValorStatus, "lblValorStatus");
            lblValorStatus.Name = "lblValorStatus";
            // 
            // lblDescSenderId
            // 
            resources.ApplyResources(lblDescSenderId, "lblDescSenderId");
            lblDescSenderId.Name = "lblDescSenderId";
            // 
            // lblDescStatus
            // 
            resources.ApplyResources(lblDescStatus, "lblDescStatus");
            lblDescStatus.Name = "lblDescStatus";
            // 
            // lblOpenedAt
            // 
            resources.ApplyResources(lblOpenedAt, "lblOpenedAt");
            lblOpenedAt.Name = "lblOpenedAt";
            // 
            // lblValorSenderId
            // 
            resources.ApplyResources(lblValorSenderId, "lblValorSenderId");
            lblValorSenderId.Name = "lblValorSenderId";
            // 
            // tabControlEngenharia
            // 
            resources.ApplyResources(tabControlEngenharia, "tabControlEngenharia");
            tabControlEngenharia.Controls.Add(tabPageVisaoGeral);
            tabControlEngenharia.Controls.Add(tabPageFinalizar);
            tabControlEngenharia.Controls.Add(tabPageEncerradas);
            tabControlEngenharia.Name = "tabControlEngenharia";
            tabControlEngenharia.SelectedIndex = 0;
            // 
            // tabPageVisaoGeral
            // 
            resources.ApplyResources(tabPageVisaoGeral, "tabPageVisaoGeral");
            tabPageVisaoGeral.Controls.Add(btnAtualizar);
            tabPageVisaoGeral.Controls.Add(grpResumoEncerradas);
            tabPageVisaoGeral.Controls.Add(grpResumoAbertas);
            tabPageVisaoGeral.Controls.Add(grpResumoNovas);
            tabPageVisaoGeral.Name = "tabPageVisaoGeral";
            tabPageVisaoGeral.UseVisualStyleBackColor = true;
            // 
            // btnAtualizar
            // 
            resources.ApplyResources(btnAtualizar, "btnAtualizar");
            btnAtualizar.Name = "btnAtualizar";
            btnAtualizar.UseVisualStyleBackColor = true;
            btnAtualizar.Click += button1_Click;
            // 
            // grpResumoEncerradas
            // 
            resources.ApplyResources(grpResumoEncerradas, "grpResumoEncerradas");
            grpResumoEncerradas.Controls.Add(lblContagemEncerradas);
            grpResumoEncerradas.Name = "grpResumoEncerradas";
            grpResumoEncerradas.TabStop = false;
            // 
            // lblContagemEncerradas
            // 
            resources.ApplyResources(lblContagemEncerradas, "lblContagemEncerradas");
            lblContagemEncerradas.Name = "lblContagemEncerradas";
            // 
            // grpResumoAbertas
            // 
            resources.ApplyResources(grpResumoAbertas, "grpResumoAbertas");
            grpResumoAbertas.Controls.Add(lblContagemAbertas);
            grpResumoAbertas.Name = "grpResumoAbertas";
            grpResumoAbertas.TabStop = false;
            // 
            // lblContagemAbertas
            // 
            resources.ApplyResources(lblContagemAbertas, "lblContagemAbertas");
            lblContagemAbertas.Name = "lblContagemAbertas";
            // 
            // grpResumoNovas
            // 
            resources.ApplyResources(grpResumoNovas, "grpResumoNovas");
            grpResumoNovas.Controls.Add(lblContagemNovas);
            grpResumoNovas.Name = "grpResumoNovas";
            grpResumoNovas.TabStop = false;
            // 
            // lblContagemNovas
            // 
            resources.ApplyResources(lblContagemNovas, "lblContagemNovas");
            lblContagemNovas.Name = "lblContagemNovas";
            // 
            // tabPageFinalizar
            // 
            resources.ApplyResources(tabPageFinalizar, "tabPageFinalizar");
            tabPageFinalizar.Controls.Add(splitContainerFinalizar);
            tabPageFinalizar.Name = "tabPageFinalizar";
            tabPageFinalizar.UseVisualStyleBackColor = true;
            // 
            // tabPageEncerradas
            // 
            resources.ApplyResources(tabPageEncerradas, "tabPageEncerradas");
            tabPageEncerradas.Controls.Add(btnAtualizarEncerradas);
            tabPageEncerradas.Controls.Add(listViewEncerradas);
            tabPageEncerradas.Name = "tabPageEncerradas";
            tabPageEncerradas.UseVisualStyleBackColor = true;
            // 
            // btnAtualizarEncerradas
            // 
            resources.ApplyResources(btnAtualizarEncerradas, "btnAtualizarEncerradas");
            btnAtualizarEncerradas.Name = "btnAtualizarEncerradas";
            btnAtualizarEncerradas.UseVisualStyleBackColor = true;
            btnAtualizarEncerradas.Click += btnAtualizarEncerradas_Click;
            // 
            // listViewEncerradas
            // 
            resources.ApplyResources(listViewEncerradas, "listViewEncerradas");
            listViewEncerradas.Columns.AddRange(new ColumnHeader[] { colClientId, colCreatedAt, colEncStatus, colDataFechado, colTempoDecorrido });
            listViewEncerradas.FullRowSelect = true;
            listViewEncerradas.GridLines = true;
            listViewEncerradas.Name = "listViewEncerradas";
            listViewEncerradas.UseCompatibleStateImageBehavior = false;
            listViewEncerradas.View = View.Details;
            // 
            // colClientId
            // 
            colClientId.Tag = "ID cliente";
            resources.ApplyResources(colClientId, "colClientId");
            // 
            // colCreatedAt
            // 
            colCreatedAt.Tag = "Abertura";
            resources.ApplyResources(colCreatedAt, "colCreatedAt");
            // 
            // colEncStatus
            // 
            colEncStatus.Tag = "Status";
            resources.ApplyResources(colEncStatus, "colEncStatus");
            // 
            // colDataFechado
            // 
            colDataFechado.Tag = "colDataFechado";
            resources.ApplyResources(colDataFechado, "colDataFechado");
            // 
            // colTempoDecorrido
            // 
            colTempoDecorrido.Tag = "colTempoDecorrido";
            resources.ApplyResources(colTempoDecorrido, "colTempoDecorrido");
            // 
            // statusStrip1
            // 
            resources.ApplyResources(statusStrip1, "statusStrip1");
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabelInfo, toolStripProgressBar });
            statusStrip1.Name = "statusStrip1";
            statusStrip1.SizingGrip = false;
            // 
            // toolStripStatusLabelInfo
            // 
            resources.ApplyResources(toolStripStatusLabelInfo, "toolStripStatusLabelInfo");
            toolStripStatusLabelInfo.Name = "toolStripStatusLabelInfo";
            toolStripStatusLabelInfo.Spring = true;
            // 
            // toolStripProgressBar
            // 
            resources.ApplyResources(toolStripProgressBar, "toolStripProgressBar");
            toolStripProgressBar.Alignment = ToolStripItemAlignment.Right;
            toolStripProgressBar.Name = "toolStripProgressBar";
            toolStripProgressBar.Style = ProgressBarStyle.Marquee;
            // 
            // Form1
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tabControlEngenharia);
            Controls.Add(statusStrip1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form1";
            Load += Form1_Load;
            splitContainerFinalizar.Panel1.ResumeLayout(false);
            splitContainerFinalizar.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerFinalizar).EndInit();
            splitContainerFinalizar.ResumeLayout(false);
            grpDetalhesMensagem.ResumeLayout(false);
            tableLayoutPanelDetalhes.ResumeLayout(false);
            tableLayoutPanelDetalhes.PerformLayout();
            tabControlEngenharia.ResumeLayout(false);
            tabPageVisaoGeral.ResumeLayout(false);
            grpResumoEncerradas.ResumeLayout(false);
            grpResumoEncerradas.PerformLayout();
            grpResumoAbertas.ResumeLayout(false);
            grpResumoAbertas.PerformLayout();
            grpResumoNovas.ResumeLayout(false);
            grpResumoNovas.PerformLayout();
            tabPageFinalizar.ResumeLayout(false);
            tabPageEncerradas.ResumeLayout(false);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TabControl tabControlEngenharia;
        private TabPage tabPageVisaoGeral;
        private TabPage tabPageFinalizar;
        private GroupBox grpResumoEncerradas;
        private GroupBox grpResumoAbertas;
        private GroupBox grpResumoNovas;
        private Label lblContagemNovas;
        private Label lblContagemAbertas;
        private Label lblContagemEncerradas;
        private Button btnAtualizar;
        private SplitContainer splitContainerFinalizar;
        private ListView listViewAbertasParaFinalizar;
        private ColumnHeader colClientName;
        private ColumnHeader colOpenAt;
        private GroupBox grpDetalhesMensagem;
        private Button btnFinalizarSelecionada;
        private TableLayoutPanel tableLayoutPanelDetalhes;
        private Label lblDescSenderId;
        private Label lblDescStatus;
        private Label lblOpenedAt;
        private Label lblValorSenderId;
        private Label lblValorOpedAt;
        private Label lblValorStatus;
        private ColumnHeader colStatus;
        private TabPage tabPageEncerradas;
        private ListView listViewEncerradas;
        private ColumnHeader colClientId;
        private ColumnHeader colCreatedAt;
        private ColumnHeader colEncStatus;
        private Button btnAtualizarEncerradas;
        private Label lblTimeOpened;
        private Label lblValorTempoAberto;
        private Button btnAtualizarAbertas;
        private ColumnHeader colDataFechado;
        private ColumnHeader colTempoDecorrido;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabelInfo;
        private ToolStripProgressBar toolStripProgressBar;
    }
}
