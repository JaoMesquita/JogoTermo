using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public class FormHistorico : Form
{
    public FormHistorico(List<string> palavras, int tentativas, TimeSpan tempo, bool acertou, string palavraCorreta)
    {
        // Configurações do Form
        this.Text = "Histórico do Jogo";
        this.Size = new Size(500, 400);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.BackColor = Color.FromArgb(30, 30, 30);
        this.ForeColor = Color.White;

        // Título
        var lblTitulo = new Label()
        {
            Text = "Histórico de Tentativas",
            Font = new Font("Consolas", 16, FontStyle.Bold),
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.MiddleCenter,
            Height = 50,
            ForeColor = Color.LightBlue
        };
        this.Controls.Add(lblTitulo);

        // Painel rolável para as tentativas
        var panelTentativas = new Panel()
        {
            Dock = DockStyle.Top,
            AutoScroll = true,
            Height = 200,
            Padding = new Padding(10),
            BackColor = Color.FromArgb(40, 40, 40)
        };
        this.Controls.Add(panelTentativas);

        // Lista de tentativas
        for (int i = palavras.Count - 1; i >= 0; i--)
        {
            var lblPalavra = new Label()
            {
                Text = $"Tentativa {i + 1}: {palavras[i]}",
                Font = new Font("Consolas", 12),
                Dock = DockStyle.Top,
                Height = 25,
                ForeColor = Color.White
            };
            panelTentativas.Controls.Add(lblPalavra);
            lblPalavra.BringToFront();
        }

        // Painel de informações
        var panelInfo = new Panel()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };
        this.Controls.Add(panelInfo);

        var lblTentativas = new Label()
        {
            Text = $"Total de tentativas: {tentativas}",
            Font = new Font("Consolas", 12),
            Dock = DockStyle.Top,
            Height = 25
        };
        panelInfo.Controls.Add(lblTentativas);

        var lblPalavraCorreta = new Label()
        {
            Text = $"Palavra correta: {palavraCorreta}",
            Font = new Font("Consolas", 12),
            Dock = DockStyle.Top,
            Height = 25
        };
        panelInfo.Controls.Add(lblPalavraCorreta);

        var lblAcertou = new Label()
        {
            Text = acertou ? "Parabéns! Você acertou a palavra!" : "Não acertou a palavra.",
            Font = new Font("Consolas", 12, FontStyle.Bold),
            ForeColor = acertou ? Color.LimeGreen : Color.Red,
            Dock = DockStyle.Top,
            Height = 30
        };
        panelInfo.Controls.Add(lblAcertou);

        var lblTempo = new Label()
        {
            Text = $"Tempo gasto: {tempo.Minutes:D2}:{tempo.Seconds:D2}", // tempo formatado MM:SS
            Font = new Font("Consolas", 12),
            Dock = DockStyle.Top,
            Height = 25,
            ForeColor = Color.Yellow
        };
        panelInfo.Controls.Add(lblTempo);

        // Botão fechar
        var btnFechar = new Button()
        {
            Text = "Fechar",
            Dock = DockStyle.Bottom,
            Height = 35,
            BackColor = Color.FromArgb(60, 60, 60),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnFechar.FlatAppearance.BorderSize = 0;
        btnFechar.Click += (s, e) => this.Close();
        this.Controls.Add(btnFechar);
    }
}
