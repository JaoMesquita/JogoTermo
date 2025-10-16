using HtmlAgilityPack;

namespace TermoLib
{
    public class Letra
    {
        public Letra(char caracter, char cor)
        {
            Caracter = caracter;
            Cor = cor;
        }

        public char Caracter;
        public char Cor;
    }

    public class Termo
    {
        public List<string> palavras = new();
        public string palavraSorteada = string.Empty;
        public List<List<Letra>> tabuleiro;
        public Dictionary<char, char> teclado;
        public int palavraAtual;
        public bool JogoFinalizado;

        public Termo()
        {
            palavraAtual = 1;
            tabuleiro = new List<List<Letra>>();
            teclado = new Dictionary<char, char>();

            // Inicializa o teclado com letras maiúsculas (A-Z)
            for (int i = 65; i <= 90; i++)
                teclado.Add((char)i, 'C');
        }

        public void CarregaPalavras(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException($"Arquivo '{fileName}' não encontrado!");

            palavras = File.ReadAllLines(fileName)
                           .Where(p => p.Length == 5)
                           .Select(p => p.ToUpper())
                           .ToList();
        }

        public void SorteiaPalavra()
        {
            if (palavras == null || palavras.Count == 0)
                throw new Exception("Lista de palavras vazia. Inicialize o jogo corretamente.");

            Random rdn = new Random();
            int index = rdn.Next(0, palavras.Count);
            palavraSorteada = palavras[index].ToUpper();
        }

        public void ChecaPalavra(string palavra)
        {
            if (string.IsNullOrEmpty(palavraSorteada) || palavraSorteada.Length != 5)
                throw new Exception("Palavra sorteada inválida! Inicialize o jogo corretamente.");

            palavra = palavra.ToUpper();

            if (palavra.Length != 5)
                throw new Exception("A palavra digitada deve ter exatamente 5 letras!");

            if (palavra == palavraSorteada)
                JogoFinalizado = true;

            var palavraTabuleiro = new List<Letra>();

            for (int i = 0; i < palavra.Length; i++)
            {
                char cor;

                if (palavra[i] == palavraSorteada[i])
                {
                    cor = 'V'; // Verde (letra correta e na posição certa)
                }
                else if (palavraSorteada.Contains(palavra[i]))
                {
                    cor = 'A'; // Amarelo (letra existe, mas posição errada)
                }
                else
                {
                    cor = 'P'; // Preto (letra inexistente)
                }

                palavraTabuleiro.Add(new Letra(palavra[i], cor));

                if (teclado.ContainsKey(palavra[i]))
                    teclado[palavra[i]] = cor;
            }

            tabuleiro.Add(palavraTabuleiro);
            palavraAtual++;
        }

        public async Task<List<string>> BaixarPalavrasDicioAsync()
        {
            var url = "https://www.dicio.com.br/palavras-com-cinco-letras/";
            var web = new HtmlWeb();
            var palavras = new List<string>();

            try
            {
                var doc = await Task.Run(() => web.Load(url));
                var nodes = doc.DocumentNode.SelectNodes("//a[@class='title']");

                // Proteção contra estrutura alterada
                if (nodes == null)
                    return new List<string>();

                foreach (var node in nodes)
                {
                    var palavra = node.InnerText.Trim();
                    if (palavra.Length == 5)
                        palavras.Add(palavra.ToUpper());
                }
            }
            catch
            {
                // Em caso de erro de conexão, retorna lista vazia
                return new List<string>();
            }

            return palavras;
        }

        public async Task InicializarComPalavrasOnlineOuArquivoAsync()
        {
            try
            {
                palavras = await BaixarPalavrasDicioAsync();

                if (palavras == null || palavras.Count == 0)
                {
                    // Fallback: tenta arquivo local
                    if (File.Exists("Palavra.txt"))
                    {
                        CarregaPalavras("Palavra.txt");
                    }
                    else
                    {
                        // Fallback final: palavras embutidas
                        palavras = new List<string>
                        {
                            "CASAS", "BOLAS", "TEMPO", "LIVRO", "MOUSE",
                            "NINJA", "PLANO", "TERMO", "CINCO", "SONHO"
                        };
                    }
                }
            }
            catch
            {
                // Qualquer exceção inesperada → fallback direto
                palavras = new List<string> { "CASAS", "BOLAS", "TEMPO", "LIVRO", "MOUSE" };
            }

            SorteiaPalavra();
        }
    }
}
