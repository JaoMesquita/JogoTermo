// FormHistorico.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace TermoApp
{
    public class FormHistorico : Form
    {
        private readonly List<string> palavras;
        private readonly int tentativas;
        private readonly TimeSpan tempo;
        private readonly bool acertou;
        private readonly string palavraCorreta;

        private System.Windows.Forms.Timer animacaoTimer = null!;
        private float gradienteOffset = 0f;

        public FormHistorico(List<string> palavras, int tentativas, TimeSpan tempo, bool acertou, string palavraCorreta)
        {
            // guarda valores
            this.palavras = palavras ?? new List<string>();
            this.tentativas = tentativas;
            this.tempo = tempo;
            this.acertou = acertou;
            this.palavraCorreta = palavraCorreta ?? "—";

            // configurações do form
            this.Text = "Histórico de Jogo";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.Black;
            this.ClientSize = new Size(480, 520);
            this.DoubleBuffered = true;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

            ConfigureLayout();
            InicializarAnimacaoGradiente();
            IniciarFadeIn();
        }

        private void ConfigureLayout()
        {
            // Título
            var lblTitulo = new Label
            {
                Text = acertou ? "🎉 Vitória!" : "🔚 Histórico",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = acertou ? Color.LimeGreen : Color.Orange,
                Dock = DockStyle.Top,
                Height = 62,
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(lblTitulo);

            // Resumo (palavra, tentativas, tempo)
            var resumoPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 92,
                Padding = new Padding(12),
                BackColor = Color.FromArgb(24, 24, 24)
            };

            var lblResumo = new Label
            {
                Text = $"Palavra correta: {palavraCorreta.ToUpper()}\n" +
                       $"Tentativas usadas: {tentativas}\n" +
                       $"Tempo total: {tempo:mm\\:ss\\.ff}",
                Font = new Font("Consolas", 11F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Color.WhiteSmoke,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            resumoPanel.Controls.Add(lblResumo);
            Controls.Add(resumoPanel);

            // Painel com lista de tentativas (com scroll)
            var panelLista = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                BackColor = Color.FromArgb(18, 18, 18)
            };

            var lst = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                HeaderStyle = ColumnHeaderStyle.None,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                FullRowSelect = false
            };

            lst.Columns.Add("Tentativa", -2, HorizontalAlignment.Left);

            // Se não houver partidas, mostra mensagem amigável
            if (palavras == null || palavras.Count == 0)
            {
                var lblSem = new Label
                {
                    Text = "Você ainda não jogou nenhuma partida.",
                    Font = new Font("Segoe UI", 12F, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                panelLista.Controls.Add(lblSem);
            }
            else
            {
                // Exibe cada tentativa em ordem (1..N)
                for (int i = 0; i < palavras.Count; i++)
                {
                    var item = new ListViewItem($"Tentativa {i + 1}:  {palavras[i]?.ToUpper() ?? ""}");
                    lst.Items.Add(item);
                }
                // Ajusta coluna para o conteúdo
                lst.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                panelLista.Controls.Add(lst);
            }

            Controls.Add(panelLista);

            // Botões na parte inferior
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 54,
                BackColor = Color.FromArgb(20, 20, 20),
                Padding = new Padding(10)
            };

            var btnFechar = new Button
            {
                Text = "Fechar",
                Dock = DockStyle.Right,
                Width = 100,
                Height = 34,
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnFechar.FlatAppearance.BorderSize = 0;
            btnFechar.Click += (s, e) => this.Close();
            btnFechar.MouseEnter += (s, e) => btnFechar.BackColor = ControlPaint.Light(btnFechar.BackColor);
            btnFechar.MouseLeave += (s, e) => btnFechar.BackColor = Color.FromArgb(70, 70, 70);
            bottomPanel.Controls.Add(btnFechar);

            Controls.Add(bottomPanel);
        }

        private void InicializarAnimacaoGradiente()
        {
            animacaoTimer = new System.Windows.Forms.Timer { Interval = 60 };
            animacaoTimer.Tick += (s, e) =>
            {
                gradienteOffset += 0.012f;
                if (gradienteOffset > 1f) gradienteOffset -= 1f;
                Invalidate();
            };
            animacaoTimer.Start();
        }

        private void IniciarFadeIn()
        {
            this.Opacity = 0;
            var fadeTimer = new System.Windows.Forms.Timer { Interval = 20 };
            fadeTimer.Tick += (s, e) =>
            {
                if (Opacity >= 1)
                {
                    fadeTimer.Stop();
                }
                else
                {
                    Opacity = Math.Min(1.0, Opacity + 0.06);
                }
            };
            fadeTimer.Start();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            var rect = this.ClientRectangle;
            if (rect.Width <= 0 || rect.Height <= 0) return;

            // Gradiente suave que "gira"
            float angle = 45f + (float)(Math.Sin(gradienteOffset * Math.PI * 2) * 30f);
            using (var brush = new LinearGradientBrush(rect, Color.FromArgb(36, 60, 90), Color.FromArgb(12, 12, 24), angle))
            {
                e.Graphics.FillRectangle(brush, rect);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                animacaoTimer?.Stop();
                animacaoTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
