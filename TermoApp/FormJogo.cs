using System.Media;
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

        public FormJogo()
        {
            InitializeComponent();
            cronometro = new System.Diagnostics.Stopwatch();
            player = new SoundPlayer(Properties.Resources.MusicAudio);
            player.PlayLooping();
            InicializaTimer();

            this.KeyPreview = true;
            this.KeyDown += FormJogo_KeyDown;
            this.AcceptButton = null;
            this.ActiveControl = null;
            this.Focus();

            this.Load += FormJogo_Load; // Adicione esta linha!
        }

        private void InicializaTimer()
        {
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 10;
            timer.Tick += Temporizador_Tick;
            timer.Start();
            cronometro.Restart();
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
        }

        private void Temporizador_Tick(object? sender, EventArgs e)
        {
            TimeSpan tempo = cronometro.Elapsed;
            lblTimer.Text = tempo.ToString(@"mm\:ss\.ff");
        }

        private void btnEnter_Click(object sender, EventArgs e)
        {
            var palavra = string.Empty;
            for (int i = 1; i <= 5; i++)
            {
                var nomeBotao = $"btn{termo.palavraAtual}{i}";
                var botao = RetornaBotao(nomeBotao);
                palavra += botao.Text;
            }

            try
            {
                termo.ChecaPalavra(palavra);
                AtualizaTabuleiro();
                coluna = 1;

                if (termo.JogoFinalizado)
                {
                    timer?.Stop();
                    MessageBox.Show("Parabéns! Você acertou a palavra!", "Jogo Termo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void AtualizaTabuleiro()
        {
            for (int col = 1; col <= 5; col++)
            {
                var letra = termo.tabuleiro[termo.palavraAtual - 2][col - 1];
                var nomeBotaoTab = $"btn{termo.palavraAtual - 1}{col}";
                var botaoTab = RetornaBotao(nomeBotaoTab);
                var nomeBotaoKey = $"btn{letra.Caracter}";
                var botaoKey = RetornaBotao(nomeBotaoKey);

                if (letra.Cor == 'A')
                {
                    botaoTab.BackColor = Color.Yellow;
                    botaoKey.BackColor = Color.Yellow;
                }
                else if (letra.Cor == 'V')
                {
                    botaoTab.BackColor = Color.Green;
                    botaoKey.BackColor = Color.Green;
                }
                else
                {
                    botaoTab.BackColor = Color.Gray;
                    botaoKey.BackColor = Color.Gray;
                }
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
            }
        }

        private void FormJogo_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnEnter_Click(this, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.Back)
            {
                btnBackspace_Click(this, EventArgs.Empty);
            }
            // Depois trata letras A-Z
            else if (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z)
            {
                if (coluna <= 5)
                {
                    // Monta o nome do botão do teclado virtual (btnA, btnB, ...)
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

        private void btnReiniciar_Click(object sender, EventArgs e)
        {
            // Reinicializa o termo (sorteia nova palavra e limpa estado)
            termo = new Termo();

            // Limpa o tabuleiro
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

            // Limpa o teclado virtual (A-Z)
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

            // Reseta timer e contador
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

        private async void FormJogo_Load(object sender, EventArgs e)
        {
            termo = new Termo();
            await termo.InicializarComPalavrasOnlineOuArquivoAsync();
            // Agora pode usar termo normalmente
        }
    }

}
