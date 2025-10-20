using System;
using System.IO;
using System.Media;
using System.Text;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TermoLib;

namespace TermoApp
{
    public partial class FormJogo : Form
    {
        public Termo termo;
        int coluna = 1;
        private System.Windows.Forms.Timer? timer;
        private System.Diagnostics.Stopwatch cronometro;
        private bool musicaLigada = true;
        private SoundPlayer? player;
        private SoundPlayer? clickPlayer; // player para efeito de clique
        private Stream? clickSoundStream;  // stream usado pelo SoundPlayer

        public FormJogo()
        {
            InitializeComponent();

            termo = new Termo();

            cronometro = new System.Diagnostics.Stopwatch();
            player = new SoundPlayer(Properties.Resources.MusicAudio);
            player.PlayLooping();

            InicializaSom();

            InicializaTimer();

            this.KeyPreview = true;
            this.KeyDown += FormJogo_KeyDown;
            this.AcceptButton = null;
            this.ActiveControl = null;
            this.Focus();

            this.Load += FormJogo_Load;
            this.Disposed += FormJogo_Disposed;

            InicializaInfo();
            InicializaHistorico();
        }

        private void FormJogo_Disposed(object? sender, EventArgs e)
        {
            try { clickPlayer?.Stop(); } catch { }
            try { clickPlayer?.Dispose(); } catch { }
            try { clickSoundStream?.Dispose(); } catch { }
        }

        private void InicializaTimer()
        {
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 10;
            timer.Tick += Temporizador_Tick;
            timer.Start();
            cronometro.Restart();
        }

        private void InicializaSom()
        {
            try
            {
                object res = Properties.Resources.ClickSound;

                if (res is UnmanagedMemoryStream ums)
                {
                    clickSoundStream = new MemoryStream();
                    ums.CopyTo(clickSoundStream);
                }
                else if (res is byte[] bytes)
                {
                    clickSoundStream = new MemoryStream(bytes);
                }
                else if (res is Stream s)
                {
                    clickSoundStream = new MemoryStream();
                    s.CopyTo(clickSoundStream);
                }
                else
                {
                    clickSoundStream = null;
                }

                if (clickSoundStream != null)
                {
                    clickSoundStream.Position = 0;
                    clickPlayer = new SoundPlayer(clickSoundStream);
                    clickPlayer.Load();
                }
            }
            catch
            {
                clickPlayer = null;
                clickSoundStream?.Dispose();
                clickSoundStream = null;
            }
        }

        private void InicializaInfo()
        {
            var btnInfoControl = Controls.Find("btnInfo", true).FirstOrDefault() as Button;
            if (btnInfoControl != null)
            {
                btnInfoControl.Click -= btnInfo_Click;
                btnInfoControl.Click += btnInfo_Click;
            }
        }

        private void InicializaHistorico()
        {
            var btnHist = Controls.Find("btnHistorico", true).FirstOrDefault() as Button;
            if (btnHist != null)
            {
                btnHist.Click -= btnHistorico_Click;
                btnHist.Click += btnHistorico_Click;
            }
        }

        private void btnTeclado_Click(object sender, EventArgs e)
        {
            if (coluna > 5) return;

            var button = (Button)sender;
            var linha = termo.palavraAtual;
            var nomeButton = $"btn{linha}{coluna}";
            var buttonTabuleiro = RetornaBotao(nomeButton);
            buttonTabuleiro.Text = button.Text;
            coluna++;

            // animação bounce
            _ = AnimarCliqueBotao(buttonTabuleiro);

            try
            {
                if (clickSoundStream != null)
                    clickSoundStream.Position = 0;
                clickPlayer?.Play();
            }
            catch { }
        }

        private void Temporizador_Tick(object? sender, EventArgs e)
        {
            TimeSpan tempo = cronometro.Elapsed;
            lblTimer.Text = tempo.ToString(@"mm\:ss\.ff");
        }

        private async void btnEnter_Click(object sender, EventArgs e)
        {
            var palavra = string.Empty;
            for (int i = 1; i <= 5; i++)
            {
                var nomeBotao = $"btn{termo.palavraAtual}{i}";
                var botao = RetornaBotao(nomeBotao);
                palavra += botao.Text;
            }

            if (string.IsNullOrWhiteSpace(palavra) || palavra.Length != 5 || palavra.Any(ch => !char.IsLetter(ch)))
            {
                MessageBox.Show("Por favor, digite 5 letras antes de enviar.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                try
                {
                    if (clickSoundStream != null)
                        clickSoundStream.Position = 0;
                    clickPlayer?.Play();
                }
                catch { }

                if (!termo.PalavraExisteNaLingua(palavra))
                {
                    MessageBox.Show("Palavra inexistente no dicionário da língua.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                termo.ChecaPalavra(palavra);
                await AtualizaTabuleiroAsync();
                coluna = 1;

                if (termo.JogoFinalizado)
                {
                    timer?.Stop();

                    await AnimarVitoriaAsync();

                    try
                    {
                        using (var winPlayer = new SoundPlayer(Properties.Resources.WinSound))
                        {
                            winPlayer.Play();
                        }
                    }
                    catch {  }

                    MessageBox.Show("🎉 Parabéns! Você acertou a palavra: " + termo.palavraSorteada,
                        "Jogo Termo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (termo.palavraAtual > 6)
                {
                    timer?.Stop();
                    MessageBox.Show("Fim de jogo! A palavra correta era: " + termo.palavraSorteada, "Jogo Termo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private Button RetornaBotao(string name)
        {
            return (Button)Controls.Find(name, true)[0];
        }

        private async Task AtualizaTabuleiroAsync()
        {
            for (int col = 1; col <= 5; col++)
            {
                var letra = termo.tabuleiro[termo.palavraAtual - 2][col - 1];
                var nomeBotaoTab = $"btn{termo.palavraAtual - 1}{col}";
                var botaoTab = RetornaBotao(nomeBotaoTab);
                var nomeBotaoKey = $"btn{letra.Caracter}";
                var botaoKey = RetornaBotao(nomeBotaoKey);

                Color corAlvo;

                if (letra.Cor == 'A') corAlvo = Color.Yellow;
                else if (letra.Cor == 'V') corAlvo = Color.Green;
                else corAlvo = Color.Gray;

                // anima flip estilo Wordle
                await AnimarFlipBotao(botaoTab, corAlvo);

                // anima teclado
                if (letra.Cor == 'P')
                    _ = AnimarPiscarBotao(botaoKey, Color.Red, corAlvo);
                else
                    botaoKey.BackColor = corAlvo;
            }
        }


        private void btnBackspace_Click(object sender, EventArgs e)
        {
            if (coluna > 1)
            {
                coluna--;
                var linha = termo.palavraAtual;
                var nomeButton = $"btn{linha}{coluna}";
                var buttonTabuleiro = RetornaBotao(nomeButton);
                buttonTabuleiro.Text = string.Empty;

                _ = AnimarCliqueBotao(buttonTabuleiro);

                try
                {
                    if (clickSoundStream != null)
                        clickSoundStream.Position = 0;
                    clickPlayer?.Play();
                }
                catch { }
            }
        }

        private void FormJogo_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnEnter_Click(this, EventArgs.Empty);
            else if (e.KeyCode == Keys.Back)
                btnBackspace_Click(this, EventArgs.Empty);
            else if (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z)
            {
                if (coluna <= 5)
                {
                    string nomeBotao = $"btn{e.KeyCode}";
                    var botoes = Controls.Find(nomeBotao, true);
                    if (botoes.Length > 0)
                        btnTeclado_Click(botoes[0], EventArgs.Empty);
                }
            }
            e.Handled = true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                btnEnter_Click(this, EventArgs.Empty);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private async void btnReiniciar_Click(object sender, EventArgs e)
        {
            termo = new Termo();
            await termo.InicializarComPalavrasDoDicionarioAsync();

            for (int linha = 1; linha <= 6; linha++)
            {
                for (int col = 1; col <= 5; col++)
                {
                    string nomeBotao = $"btn{linha}{col}";
                    var botoes = Controls.Find(nomeBotao, true);
                    if (botoes.Length > 0)
                    {
                        var botao = (Button)botoes[0];
                        botao.Text = string.Empty;
                        botao.BackColor = SystemColors.ControlLight;
                    }
                }
            }

            for (char c = 'A'; c <= 'Z'; c++)
            {
                string nomeBotao = $"btn{c}";
                var botoes = Controls.Find(nomeBotao, true);
                if (botoes.Length > 0)
                {
                    var botao = (Button)botoes[0];
                    botao.BackColor = SystemColors.ButtonFace;
                }
            }

            lblTimer.Text = "00:00";
            cronometro.Restart();
            if (timer != null)
            {
                timer.Stop();
                timer.Start();
            }
            coluna = 1;
        }

        private void IconMusic_Click(object sender, EventArgs e)
        {
            musicaLigada = !musicaLigada;
            if (musicaLigada)
            {
                IconMusic.Image = Properties.Resources.MusicOn;
                player?.PlayLooping();
            }
            else
            {
                IconMusic.Image = Properties.Resources.musicOff;
                player?.Stop();
            }
        }

        private async void FormJogo_Load(object? sender, EventArgs e)
        {
            termo = new Termo();
            await termo.InicializarComPalavrasDoDicionarioAsync();
        }

        private void btnInfo_Click(object? sender, EventArgs e)
        {
            var autor = "João Mesquita";
            var contato = "j.mesquita@aluno.ifsp.edu.br";
            var sobre = @"Termo — Jogo estilo Wordle em português.
            - Chute palavras de 5 letras.
            - Letras corretas na posição ficam verdes.
            - Letras existentes em outra posição ficam amarelas.
            - Letras inexistentes ficam cinzas.
            - Você tem 6 tentativas.";

            var mensagem = new StringBuilder();
            mensagem.AppendLine($"Desenvolvedor: {autor}");
            mensagem.AppendLine($"Contato: {contato}");
            mensagem.AppendLine();
            mensagem.AppendLine(sobre);
            mensagem.AppendLine();
            mensagem.AppendLine("Boa sorte e divirta-se!");

            MessageBox.Show(mensagem.ToString(), "Sobre o Jogo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHistorico_Click(object? sender, EventArgs e)
        {
            if (!termo.JogoFinalizado && termo.palavraAtual <= 6)
            {
                MessageBox.Show("O histórico só está disponível após o jogo terminar.", "Atenção",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var palavras = new List<string>();
            if (termo.tabuleiro != null)
            {
                foreach (var row in termo.tabuleiro)
                {
                    var sb = new StringBuilder();
                    foreach (var letra in row)
                        sb.Append(letra.Caracter);
                    palavras.Add(sb.ToString());
                }
            }

            int tentativas = Math.Max(0, termo.palavraAtual - 1);
            bool acertou = termo.JogoFinalizado;
            var tempo = cronometro.Elapsed;

            using var f = new FormHistorico(palavras, tentativas, tempo, acertou, termo.palavraSorteada);
            f.ShowDialog(this);
        }

        private async Task AnimarCliqueBotao(Button botao)
        {
            var tamanhoOriginal = botao.Font.Size;
            for (int i = 0; i < 2; i++)
            {
                botao.Font = new Font(botao.Font.FontFamily, tamanhoOriginal + 4, botao.Font.Style);
                await Task.Delay(50);
                botao.Font = new Font(botao.Font.FontFamily, tamanhoOriginal, botao.Font.Style);
                await Task.Delay(50);
            }
        }

        private async Task AnimarFlipBotao(Button botao, Color corFinal)
        {
            int passos = 8;
            var corOriginal = botao.BackColor;
            int alturaOriginal = botao.Height;

            for (int i = 0; i < passos; i++)
            {
                // estreita verticalmente simulando rotação
                botao.Height = alturaOriginal - (alturaOriginal * i / passos);
                await Task.Delay(30);
            }

            // troca a cor da letra enquanto “vira”
            botao.BackColor = corFinal;

            for (int i = passos; i >= 0; i--)
            {
                // expande verticalmente simulando giro completo
                botao.Height = alturaOriginal - (alturaOriginal * i / passos);
                await Task.Delay(30);
            }

            botao.Height = alturaOriginal; // garante altura final correta
        }


        private async Task AnimarPiscarBotao(Button botao, Color cor1, Color cor2, int vezes = 3)
        {
            for (int i = 0; i < vezes; i++)
            {
                botao.BackColor = cor1;
                await Task.Delay(150);
                botao.BackColor = cor2;
                await Task.Delay(150);
            }
        }

        private async Task AnimarVitoriaAsync()
        {
            int linhaFinal = termo.palavraAtual - 1;

            // todos os botões da linha final (palavra correta)
            var botoes = Enumerable.Range(1, 5)
                .Select(i => RetornaBotao($"btn{linhaFinal}{i}"))
                .ToList();

            // anima todos com efeito bounce + brilho
            for (int r = 0; r < 3; r++)
            {
                foreach (var b in botoes)
                {
                    await Task.Delay(40);
                    _ = AnimarBounceVerde(b);
                }
                await Task.Delay(250);
            }
        }

        private async Task AnimarBounceVerde(Button botao)
        {
            Color original = botao.BackColor;
            var tamanhoOriginal = botao.Font.Size;

            // Efeito “pulo” com flash verde
            for (int i = 0; i < 2; i++)
            {
                botao.BackColor = Color.LimeGreen;
                botao.Font = new Font(botao.Font.FontFamily, tamanhoOriginal + 6, botao.Font.Style);
                await Task.Delay(80);
                botao.Font = new Font(botao.Font.FontFamily, tamanhoOriginal, botao.Font.Style);
                botao.BackColor = original;
                await Task.Delay(80);
            }
        }

    }
}
