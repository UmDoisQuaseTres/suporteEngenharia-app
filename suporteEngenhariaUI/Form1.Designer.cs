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
            clientName = new ColumnHeader();
            Date = new ColumnHeader();
            grpDetalhesMensagem = new GroupBox();
            btnFinalizarSelecionada = new Button();
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
            SuspendLayout();
            // 
            // tabControlEngenharia
            // 
            tabControlEngenharia.Controls.Add(tabPageVisaoGeral);
            tabControlEngenharia.Controls.Add(tabPageFinalizar);
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
            // 
            // grpResumoEncerradas
            // 
            grpResumoEncerradas.Controls.Add(lblContagemEncerradas);
            grpResumoEncerradas.Location = new Point(262, 52);
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
            lblContagemAbertas.Click += label2_Click;
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
            tabPageFinalizar.Text = "Finalizar conversa";
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
            splitContainerFinalizar.SplitterDistance = 194;
            splitContainerFinalizar.TabIndex = 0;
            // 
            // listViewAbertasParaFinalizar
            // 
            listViewAbertasParaFinalizar.Columns.AddRange(new ColumnHeader[] { clientName, Date });
            listViewAbertasParaFinalizar.Dock = DockStyle.Fill;
            listViewAbertasParaFinalizar.FullRowSelect = true;
            listViewAbertasParaFinalizar.GridLines = true;
            listViewAbertasParaFinalizar.Location = new Point(0, 0);
            listViewAbertasParaFinalizar.Name = "listViewAbertasParaFinalizar";
            listViewAbertasParaFinalizar.Size = new Size(194, 283);
            listViewAbertasParaFinalizar.TabIndex = 0;
            listViewAbertasParaFinalizar.UseCompatibleStateImageBehavior = false;
            listViewAbertasParaFinalizar.View = View.Details;
            listViewAbertasParaFinalizar.SelectedIndexChanged += listViewAbertasParaFinalizar_SelectedIndexChanged;
            // 
            // clientName
            // 
            clientName.Tag = "Nome cliente";
            // 
            // Date
            // 
            Date.Tag = "Data de abertura";
            // 
            // grpDetalhesMensagem
            // 
            grpDetalhesMensagem.Dock = DockStyle.Top;
            grpDetalhesMensagem.Location = new Point(0, 0);
            grpDetalhesMensagem.Name = "grpDetalhesMensagem";
            grpDetalhesMensagem.Size = new Size(385, 100);
            grpDetalhesMensagem.TabIndex = 0;
            grpDetalhesMensagem.TabStop = false;
            grpDetalhesMensagem.Text = "Detalhes da mensagem selecionada";
            // 
            // btnFinalizarSelecionada
            // 
            btnFinalizarSelecionada.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnFinalizarSelecionada.Enabled = false;
            btnFinalizarSelecionada.Location = new Point(243, 244);
            btnFinalizarSelecionada.Name = "btnFinalizarSelecionada";
            btnFinalizarSelecionada.Size = new Size(137, 34);
            btnFinalizarSelecionada.TabIndex = 1;
            btnFinalizarSelecionada.Text = "Finalizar selecionada(s)";
            btnFinalizarSelecionada.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(597, 317);
            Controls.Add(tabControlEngenharia);
            Name = "Form1";
            Text = "Suporte engenharia - Controle";
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
        private ColumnHeader clientName;
        private ColumnHeader Date;
        private GroupBox grpDetalhesMensagem;
        private Button btnFinalizarSelecionada;
    }
}
