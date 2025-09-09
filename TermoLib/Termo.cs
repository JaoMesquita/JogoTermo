namespace TermoLib
{

    public class letra
    {
        public letra(char caracter, char cor)
        {
            Caracter = caracter;
            Cor = cor;
        }
        public char Caracter;
        public char Cor;
    }
    public class Termo
    {
        public List<string> palavras;
        public string palavraSorteada;
        public List<List<letra>> tabuleiro;
        public Dictionary<char, char> teclado;
        public int palavraAtual;
        public bool JogoFinalizado;

        public Termo()
        {
            CarregaPalavras("Palavra.txt");
            SorteiaPalavra();
            palavraAtual = 1;
            tabuleiro = new List<List<letra>>();
            teclado = new Dictionary<char, char> ();
            for(int i=65; i<=90; i++)
            {
                // C - NÃO DIGITADO | V - POSIÇÃO CORRETA | A - NA PALAVRA | P - NÃO FAZ PARTE
                teclado.Add((char)i, 'C');
            }

        }

        public void CarregaPalavras(string fileName)
        {
            palavras = File.ReadAllLines(fileName).ToList();

        }
        public void SorteiaPalavra()
        {
            Random rdn = new Random();
            var index = rdn.Next(0, palavras.Count() - 1);
            palavraSorteada = palavras[index];
        }

        public void ChecaPalavra(string palavra)
        {
            if (palavra == palavraSorteada)
                JogoFinalizado = true;
            if (palavra.Length != 5)
                throw new Exception("Palavra com tamanho incorreto!");

            var palavraTabuleiro = new List<letra>();
            char cor = 'V';

            for(int i = 0; i < palavra.Length; i++)
            {
                if (palavra[i] == palavraSorteada[i])
                {
                    cor = 'V';
                }
                else if (palavraSorteada.Contains(palavra[i]))
                {
                    cor = 'A'; // comentario
                }
                else
                {
                    cor = 'P';
                }
                palavraTabuleiro.Add(new letra(palavra[i], cor));
                teclado[palavra[i]] = cor;
            }
            tabuleiro.Add(palavraTabuleiro);
            palavraAtual++;
        }
    }
}
