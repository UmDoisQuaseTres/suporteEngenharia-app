namespace suporteEngenhariaUI
{
    partial class Form1
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
            ColumnHeader colSenderId;
            tabControlEngenharia = new TabControl();
            tabPageVisaoGeral = new TabPage();
            button1 = new Button();
            grpResumoEncerradas = new GroupBox();
            lblContagemEncerradas = new Label();
            grpResumoAbertas = new GroupBox();
            lblContagemAbertas = new Label();
            grpResumoNovas = new GroupBox();
            lblContagemNovas = new Label();
            tabPageFinalizar = new TabPage();
            splitContainerFinalizar = new SplitContainer();
            listViewAbertasParaFinalizar = new ListView();
            colLastUpdate = new ColumnHeader();
            colStatus = new ColumnHeader();
            btnFinalizarSelecionada = new Button();
            grpDetalhesMensagem = new GroupBox();
            tableLayoutPanelDetalhes = new TableLayoutPanel();
            lblValorLastUpdate = new Label();
            lblValorStatus = new Label();
            lblDescSenderId = new Label();
            lblDescStatus = new Label();
            lblDescLastUpdate = new Label();
            lblValorSenderId = new Label();
            tabPageEncerradas = new TabPage();
            btnAtualizarEncerradas = new Button();
            listViewEncerradas = new ListView();
            colEncSenderId = new ColumnHeader();
            colEncLastUpdate = new ColumnHeader();
            colEncStatus = new ColumnHeader();
            colSenderId = new ColumnHeader();
            tabControlEngenharia.SuspendLayout();
            tabPageVisaoGeral.SuspendLayout();
            grpResumoEncerradas.SuspendLayout();
            grpResumoAbertas.SuspendLayout();
            grpResumoNovas.SuspendLayout();
            tabPageFinalizar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerFinalizar).BeginInit();
            splitContainerFinalizar.Panel1.SuspendLayout();
            splitContainerFinalizar.Panel2.SuspendLayout();
            splitContainerFinalizar.SuspendLayout();
            grpDetalhesMensagem.SuspendLayout();
            tableLayoutPanelDetalhes.SuspendLayout();
            tabPageEncerradas.SuspendLayout();
            SuspendLayout();
            // 
            // colSenderId
            // 
            colSenderId.Tag = "ID Remetente";
            colSenderId.Text = "Nº Remetente";
            colSenderId.Width = 100;
            // 
            // tabControlEngenharia
            // 
            tabControlEngenharia.Controls.Add(tabPageVisaoGeral);
            tabControlEngenharia.Controls.Add(tabPageFinalizar);
            tabControlEngenharia.Controls.Add(tabPageEncerradas);
            tabControlEngenharia.Dock = DockStyle.Fill;
            tabControlEngenharia.Location = new Point(0, 0);
            tabControlEngenharia.Name = "tabControlEngenharia";
            tabControlEngenharia.SelectedIndex = 0;
            tabControlEngenharia.Size = new Size(597, 317);
            tabControlEngenharia.TabIndex = 0;
            // 
            // tabPageVisaoGeral
            // 
            tabPageVisaoGeral.Controls.Add(button1);
            tabPageVisaoGeral.Controls.Add(grpResumoEncerradas);
            tabPageVisaoGeral.Controls.Add(grpResumoAbertas);
            tabPageVisaoGeral.Controls.Add(grpResumoNovas);
            tabPageVisaoGeral.Location = new Point(4, 24);
            tabPageVisaoGeral.Name = "tabPageVisaoGeral";
            tabPageVisaoGeral.Padding = new Padding(3);
            tabPageVisaoGeral.Size = new Size(589, 289);
            tabPageVisaoGeral.TabIndex = 0;
            tabPageVisaoGeral.Text = "Visão geral";
            tabPageVisaoGeral.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.Location = new Point(391, 231);
            button1.Name = "button1";
            button1.Size = new Size(119, 40);
            button1.TabIndex = 3;
            button1.Text = "Atualizar";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // grpResumoEncerradas
            // 
            grpResumoEncerradas.Controls.Add(lblContagemEncerradas);
            grpResumoEncerradas.Location = new Point(333, 71);
            grpResumoEncerradas.Name = "grpResumoEncerradas";
            grpResumoEncerradas.Size = new Size(248, 100);
            grpResumoEncerradas.TabIndex = 2;
            grpResumoEncerradas.TabStop = false;
            grpResumoEncerradas.Text = "Mensagens encerradas";
            // 
            // lblContagemEncerradas
            // 
            lblContagemEncerradas.AutoSize = true;
            lblContagemEncerradas.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblContagemEncerradas.Location = new Point(6, 19);
            lblContagemEncerradas.Name = "lblContagemEncerradas";
            lblContagemEncerradas.Size = new Size(110, 21);
            lblContagemEncerradas.TabIndex = 1;
            lblContagemEncerradas.Text = "Encerradas: 0";
            // 
            // grpResumoAbertas
            // 
            grpResumoAbertas.Controls.Add(lblContagemAbertas);
            grpResumoAbertas.Location = new Point(8, 112);
            grpResumoAbertas.Name = "grpResumoAbertas";
            grpResumoAbertas.Size = new Size(248, 100);
            grpResumoAbertas.TabIndex = 1;
            grpResumoAbertas.TabStop = false;
            grpResumoAbertas.Text = "Mensagens em aberto";
            // 
            // lblContagemAbertas
            // 
            lblContagemAbertas.AutoSize = true;
            lblContagemAbertas.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblContagemAbertas.Location = new Point(6, 19);
            lblContagemAbertas.Name = "lblContagemAbertas";
            lblContagemAbertas.Size = new Size(105, 21);
            lblContagemAbertas.TabIndex = 1;
            lblContagemAbertas.Text = "Em aberto: 0";
            // 
            // grpResumoNovas
            // 
            grpResumoNovas.Controls.Add(lblContagemNovas);
            grpResumoNovas.Location = new Point(8, 6);
            grpResumoNovas.Name = "grpResumoNovas";
            grpResumoNovas.Size = new Size(248, 100);
            grpResumoNovas.TabIndex = 0;
            grpResumoNovas.TabStop = false;
            grpResumoNovas.Text = "Novas mensagens";
            // 
            // lblContagemNovas
            // 
            lblContagemNovas.AutoSize = true;
            lblContagemNovas.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblContagemNovas.Location = new Point(10, 21);
            lblContagemNovas.Name = "lblContagemNovas";
            lblContagemNovas.Size = new Size(75, 21);
            lblContagemNovas.TabIndex = 0;
            lblContagemNovas.Text = "Novas: 0";
            // 
            // tabPageFinalizar
            // 
            tabPageFinalizar.Controls.Add(splitContainerFinalizar);
            tabPageFinalizar.Location = new Point(4, 24);
            tabPageFinalizar.Name = "tabPageFinalizar";
            tabPageFinalizar.Padding = new Padding(3);
            tabPageFinalizar.Size = new Size(589, 289);
            tabPageFinalizar.TabIndex = 1;
            tabPageFinalizar.Text = "Conversas abertas";
            tabPageFinalizar.UseVisualStyleBackColor = true;
            // 
            // splitContainerFinalizar
            // 
            splitContainerFinalizar.Dock = DockStyle.Fill;
            splitContainerFinalizar.Location = new Point(3, 3);
            splitContainerFinalizar.Name = "splitContainerFinalizar";
            // 
            // splitContainerFinalizar.Panel1
            // 
            splitContainerFinalizar.Panel1.Controls.Add(listViewAbertasParaFinalizar);
            // 
            // splitContainerFinalizar.Panel2
            // 
            splitContainerFinalizar.Panel2.Controls.Add(btnFinalizarSelecionada);
            splitContainerFinalizar.Panel2.Controls.Add(grpDetalhesMensagem);
            splitContainerFinalizar.Size = new Size(583, 283);
            splitContainerFinalizar.SplitterDistance = 275;
            splitContainerFinalizar.TabIndex = 0;
            // 
            // listViewAbertasParaFinalizar
            // 
            listViewAbertasParaFinalizar.Columns.AddRange(new ColumnHeader[] { colSenderId, colLastUpdate, colStatus });
            listViewAbertasParaFinalizar.Dock = DockStyle.Fill;
            listViewAbertasParaFinalizar.FullRowSelect = true;
            listViewAbertasParaFinalizar.GridLines = true;
            listViewAbertasParaFinalizar.Location = new Point(0, 0);
            listViewAbertasParaFinalizar.Name = "listViewAbertasParaFinalizar";
            listViewAbertasParaFinalizar.Size = new Size(275, 283);
            listViewAbertasParaFinalizar.TabIndex = 0;
            listViewAbertasParaFinalizar.UseCompatibleStateImageBehavior = false;
            listViewAbertasParaFinalizar.View = View.Details;
            listViewAbertasParaFinalizar.SelectedIndexChanged += listViewAbertasParaFinalizar_SelectedIndexChanged;
            // 
            // colLastUpdate
            // 
            colLastUpdate.Tag = "Última Atualização";
            colLastUpdate.Text = "Ultima atualização";
            colLastUpdate.Width = 100;
            // 
            // colStatus
            // 
            colStatus.Tag = "Status";
            colStatus.Text = "Status";
            colStatus.Width = 100;
            // 
            // btnFinalizarSelecionada
            // 
            btnFinalizarSelecionada.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnFinalizarSelecionada.Enabled = false;
            btnFinalizarSelecionada.Location = new Point(162, 244);
            btnFinalizarSelecionada.Name = "btnFinalizarSelecionada";
            btnFinalizarSelecionada.Size = new Size(137, 34);
            btnFinalizarSelecionada.TabIndex = 1;
            btnFinalizarSelecionada.Text = "Finalizar selecionada(s)";
            btnFinalizarSelecionada.UseVisualStyleBackColor = true;
            btnFinalizarSelecionada.Click += btnFinalizarSelecionada_Click;
            // 
            // grpDetalhesMensagem
            // 
            grpDetalhesMensagem.Controls.Add(tableLayoutPanelDetalhes);
            grpDetalhesMensagem.Dock = DockStyle.Top;
            grpDetalhesMensagem.Location = new Point(0, 0);
            grpDetalhesMensagem.Name = "grpDetalhesMensagem";
            grpDetalhesMensagem.Size = new Size(304, 112);
            grpDetalhesMensagem.TabIndex = 0;
            grpDetalhesMensagem.TabStop = false;
            grpDetalhesMensagem.Text = "Detalhes da mensagem selecionada";
            // 
            // tableLayoutPanelDetalhes
            // 
            tableLayoutPanelDetalhes.ColumnCount = 2;
            tableLayoutPanelDetalhes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanelDetalhes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanelDetalhes.Controls.Add(lblValorLastUpdate, 1, 2);
            tableLayoutPanelDetalhes.Controls.Add(lblValorStatus, 1, 1);
            tableLayoutPanelDetalhes.Controls.Add(lblDescSenderId, 0, 0);
            tableLayoutPanelDetalhes.Controls.Add(lblDescStatus, 0, 1);
            tableLayoutPanelDetalhes.Controls.Add(lblDescLastUpdate, 0, 2);
            tableLayoutPanelDetalhes.Controls.Add(lblValorSenderId, 1, 0);
            tableLayoutPanelDetalhes.Dock = DockStyle.Fill;
            tableLayoutPanelDetalhes.Location = new Point(3, 19);
            tableLayoutPanelDetalhes.Name = "tableLayoutPanelDetalhes";
            tableLayoutPanelDetalhes.RowCount = 3;
            tableLayoutPanelDetalhes.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanelDetalhes.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanelDetalhes.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanelDetalhes.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanelDetalhes.Size = new Size(298, 90);
            tableLayoutPanelDetalhes.TabIndex = 0;
            // 
            // lblValorLastUpdate
            // 
            lblValorLastUpdate.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblValorLastUpdate.Location = new Point(152, 68);
            lblValorLastUpdate.Name = "lblValorLastUpdate";
            lblValorLastUpdate.Size = new Size(143, 14);
            lblValorLastUpdate.TabIndex = 6;
            lblValorLastUpdate.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblValorStatus
            // 
            lblValorStatus.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblValorStatus.Location = new Point(152, 38);
            lblValorStatus.Name = "lblValorStatus";
            lblValorStatus.Size = new Size(143, 14);
            lblValorStatus.TabIndex = 5;
            lblValorStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblDescSenderId
            // 
            lblDescSenderId.Anchor = AnchorStyles.Left;
            lblDescSenderId.AutoSize = true;
            lblDescSenderId.Location = new Point(3, 7);
            lblDescSenderId.Name = "lblDescSenderId";
            lblDescSenderId.Size = new Size(81, 15);
            lblDescSenderId.TabIndex = 0;
            lblDescSenderId.Text = "Remetente ID:";
            lblDescSenderId.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblDescStatus
            // 
            lblDescStatus.Anchor = AnchorStyles.Left;
            lblDescStatus.AutoSize = true;
            lblDescStatus.Location = new Point(3, 37);
            lblDescStatus.Name = "lblDescStatus";
            lblDescStatus.Size = new Size(42, 15);
            lblDescStatus.TabIndex = 1;
            lblDescStatus.Text = "Status:";
            lblDescStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblDescLastUpdate
            // 
            lblDescLastUpdate.Anchor = AnchorStyles.Left;
            lblDescLastUpdate.AutoSize = true;
            lblDescLastUpdate.Location = new Point(3, 67);
            lblDescLastUpdate.Name = "lblDescLastUpdate";
            lblDescLastUpdate.Size = new Size(63, 15);
            lblDescLastUpdate.TabIndex = 2;
            lblDescLastUpdate.Text = "Última At.:";
            lblDescLastUpdate.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblValorSenderId
            // 
            lblValorSenderId.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblValorSenderId.Location = new Point(152, 8);
            lblValorSenderId.Name = "lblValorSenderId";
            lblValorSenderId.Size = new Size(143, 14);
            lblValorSenderId.TabIndex = 4;
            lblValorSenderId.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // tabPageEncerradas
            // 
            tabPageEncerradas.Controls.Add(btnAtualizarEncerradas);
            tabPageEncerradas.Controls.Add(listViewEncerradas);
            tabPageEncerradas.Location = new Point(4, 24);
            tabPageEncerradas.Name = "tabPageEncerradas";
            tabPageEncerradas.Size = new Size(589, 289);
            tabPageEncerradas.TabIndex = 2;
            tabPageEncerradas.Text = "Conversas finalizadas";
            tabPageEncerradas.UseVisualStyleBackColor = true;
            // 
            // btnAtualizarEncerradas
            // 
            btnAtualizarEncerradas.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAtualizarEncerradas.Location = new Point(424, 251);
            btnAtualizarEncerradas.Name = "btnAtualizarEncerradas";
            btnAtualizarEncerradas.Size = new Size(157, 30);
            btnAtualizarEncerradas.TabIndex = 1;
            btnAtualizarEncerradas.Text = "Atualizar Lista Encerradas";
            btnAtualizarEncerradas.UseVisualStyleBackColor = true;
            btnAtualizarEncerradas.Click += btnAtualizarEncerradas_Click;
            // 
            // listViewEncerradas
            // 
            listViewEncerradas.Columns.AddRange(new ColumnHeader[] { colEncSenderId, colEncLastUpdate, colEncStatus });
            listViewEncerradas.Dock = DockStyle.Fill;
            listViewEncerradas.FullRowSelect = true;
            listViewEncerradas.GridLines = true;
            listViewEncerradas.Location = new Point(0, 0);
            listViewEncerradas.Name = "listViewEncerradas";
            listViewEncerradas.Size = new Size(589, 289);
            listViewEncerradas.TabIndex = 0;
            listViewEncerradas.UseCompatibleStateImageBehavior = false;
            listViewEncerradas.View = View.Details;
            // 
            // colEncSenderId
            // 
            colEncSenderId.Tag = "ID Remetente";
            colEncSenderId.Text = "ID Remetente";
            colEncSenderId.Width = 120;
            // 
            // colEncLastUpdate
            // 
            colEncLastUpdate.Tag = "Última Atualização";
            colEncLastUpdate.Text = "Última Atualização";
            colEncLastUpdate.Width = 120;
            // 
            // colEncStatus
            // 
            colEncStatus.Tag = "Status";
            colEncStatus.Text = "Status";
            colEncStatus.Width = 120;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(597, 317);
            Controls.Add(tabControlEngenharia);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form1";
            Text = "Suporte engenharia - Controle";
            Load += Form1_Load;
            tabControlEngenharia.ResumeLayout(false);
            tabPageVisaoGeral.ResumeLayout(false);
            grpResumoEncerradas.ResumeLayout(false);
            grpResumoEncerradas.PerformLayout();
            grpResumoAbertas.ResumeLayout(false);
            grpResumoAbertas.PerformLayout();
            grpResumoNovas.ResumeLayout(false);
            grpResumoNovas.PerformLayout();
            tabPageFinalizar.ResumeLayout(false);
            splitContainerFinalizar.Panel1.ResumeLayout(false);
            splitContainerFinalizar.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerFinalizar).EndInit();
            splitContainerFinalizar.ResumeLayout(false);
            grpDetalhesMensagem.ResumeLayout(false);
            tableLayoutPanelDetalhes.ResumeLayout(false);
            tableLayoutPanelDetalhes.PerformLayout();
            tabPageEncerradas.ResumeLayout(false);
            ResumeLayout(false);
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
        private Button button1;
        private SplitContainer splitContainerFinalizar;
        private ListView listViewAbertasParaFinalizar;
        private ColumnHeader colSenderId;
        private ColumnHeader colLastUpdate;
        private GroupBox grpDetalhesMensagem;
        private Button btnFinalizarSelecionada;
        private TableLayoutPanel tableLayoutPanelDetalhes;
        private Label lblDescSenderId;
        private Label lblDescStatus;
        private Label lblDescLastUpdate;
        private Label lblValorSenderId;
        private Label lblValorLastUpdate;
        private Label lblValorStatus;
        private ColumnHeader colStatus;
        private TabPage tabPageEncerradas;
        private ListView listViewEncerradas;
        private ColumnHeader colEncSenderId;
        private ColumnHeader colEncLastUpdate;
        private ColumnHeader colEncStatus;
        private Button btnAtualizarEncerradas;
    }
}
