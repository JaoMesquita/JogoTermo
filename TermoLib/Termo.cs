using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;

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
        private HashSet<string> palavrasSet = new(StringComparer.OrdinalIgnoreCase); // para validação rápida (palavras do jogo)
        private HashSet<string> confereSet = new(StringComparer.OrdinalIgnoreCase);   // dicionário amplo para conferir existência
        public string palavraSorteada = string.Empty;
        public List<List<Letra>> tabuleiro;
        public Dictionary<char, char> teclado;
        public int palavraAtual;
        public bool JogoFinalizado;

        public Termo()
        {
            palavraAtual = 1;
            tabuleiro = new List<List<Letra>>();
            teclado = new Dictionary<char, char> ();
            for (int i = 65; i <= 90; i++)
            {
                teclado.Add((char)i, 'C');
            }
        }

        public void CarregaPalavras(string fileName)
        {
            // Se arquivo não existir, deixa lista vazia (chamador tratará fallback)
            if (!File.Exists(fileName))
            {
                palavras = new List<string>();
                palavrasSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                return;
            }

            // Normaliza para maiúsculas, remove acentos e mantém apenas palavras com 5 letras
            var cleaned = File.ReadAllLines(fileName)
                              .Select(line => CleanWord(line))
                              .Where(w => w.Length == 5)
                              .Distinct(StringComparer.OrdinalIgnoreCase)
                              .ToList();

            // Atualiza coleções
            palavras = cleaned;
            palavrasSet = new HashSet<string>(cleaned, StringComparer.OrdinalIgnoreCase);

            // Regrava o arquivo com as palavras limpas (opcional, mantém dicionário consistente)
            try
            {
                File.WriteAllLines(fileName, palavras);
            }
            catch
            {
                // não propaga — chamador deve tratar se necessário
            }
        }

        // Carrega arquivo grande de conferência (DicionarioConfere.txt).
        // Mantém apenas palavras limpas de 5 letras (pois o jogo trabalha com 5 letras).
        public void CarregaDicionarioConfere(string fileName)
        {
            // Assume que o arquivo já está pré-formatado: uma palavra por linha,
            // maiúsculas e com exatamente 5 caracteres. Apenas carrega tal como está.
            confereSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!File.Exists(fileName))
            {
                // não lança — chamador pode decidir o que fazer
                return;
            }

            var lines = File.ReadAllLines(fileName)
                            .Select(l => l.Trim().ToUpperInvariant())
                            .Where(l => l.Length == 5) // mantém apenas linhas com 5 caracteres
                            .Distinct(StringComparer.OrdinalIgnoreCase);

            confereSet = new HashSet<string>(lines, StringComparer.OrdinalIgnoreCase);
        }

        public void SorteiaPalavra()
        {
            if (palavras == null || palavras.Count == 0)
                throw new InvalidOperationException("Lista de palavras vazia. Inicialize o jogo com um dicionário válido.");

            var index = Random.Shared.Next(palavras.Count);
            palavraSorteada = palavras[index];
        }

        // Verifica se a palavra existe no dicionário do jogo (lista carregada)
        public bool PalavraValida(string palavra)
        {
            if (string.IsNullOrWhiteSpace(palavra))
                return false;

            var p = CleanWord(palavra);
            return p.Length == 5 && palavrasSet != null && palavrasSet.Contains(p);
        }

        // Verifica existência no dicionário amplo (DicionarioConfere.txt)
        public bool PalavraExisteNaLingua(string palavra)
        {
            if (string.IsNullOrWhiteSpace(palavra))
                return false;

            var p = CleanWord(palavra);
            return p.Length == 5 && confereSet != null && confereSet.Contains(p);
        }

        public void ChecaPalavra(string palavra)
        {
            if (string.IsNullOrEmpty(palavraSorteada) || palavraSorteada.Length != 5)
                throw new InvalidOperationException("Palavra sorteada inválida.");

            var p = CleanWord(palavra);

            if (p.Length != 5)
                throw new Exception("Palavra com tamanho incorreto!");

            if (!PalavraExisteNaLingua(p))
                throw new Exception("Palavra inexistente no dicionário!");

            if (p == palavraSorteada)
                JogoFinalizado = true;

            var palavraTabuleiro = new List<Letra>();

            // Conta quantas vezes cada letra aparece na palavra sorteada
            var contagem = new Dictionary<char, int>();
            foreach (var ch in palavraSorteada)
            {
                if (!contagem.ContainsKey(ch)) contagem[ch] = 0;
                contagem[ch]++;
            }

            // 1º Passo: marcar verdes
            for (int i = 0; i < p.Length; i++)
            {
                if (p[i] == palavraSorteada[i])
                {
                    palavraTabuleiro.Add(new Letra(p[i], 'V'));
                    contagem[p[i]]--; // usa uma instância da letra
                }
                else
                {
                    palavraTabuleiro.Add(new Letra(p[i], ' ')); // ainda não decidido
                }
            }

            // 2º Passo: marcar amarelas e cinzas
            for (int i = 0; i < p.Length; i++)
            {
                if (palavraTabuleiro[i].Cor == ' ') // não verde
                {
                    if (contagem.ContainsKey(p[i]) && contagem[p[i]] > 0)
                    {
                        palavraTabuleiro[i].Cor = 'A';
                        contagem[p[i]]--; // usa uma instância da letra
                    }
                    else
                    {
                        palavraTabuleiro[i].Cor = 'P';
                    }
                }
            }

            // Atualiza tabuleiro e teclado
            for (int i = 0; i < 5; i++)
            {
                var letra = palavraTabuleiro[i];
                teclado[letra.Caracter] = letra.Cor;
            }

            tabuleiro.Add(palavraTabuleiro);
            palavraAtual++;
        }


        public async Task InicializarComPalavrasDoDicionarioAsync()
        {
            // Garante que exista um arquivo "Dicionario.txt" válido antes de carregar
            GarantirArquivoPalavras("Dicionario.txt", minWords: 10);
            CarregaPalavras("Dicionario.txt");

            // Carrega o dicionário de conferência (arquivo grande com todas as palavras)
            CarregaDicionarioConfere("DicionarioConfere.txt");

            // Se algum dos dicionários essenciais estiver vazio, lançar erro claro para o chamador (Form deve tratar)
            if (palavras == null || palavras.Count == 0)
                throw new InvalidOperationException("Não foi possível carregar palavras do arquivo local (Dicionario.txt).");

            if (confereSet == null || confereSet.Count == 0)
                throw new InvalidOperationException("Não foi possível carregar o arquivo 'DicionarioConfere.txt' (ou está vazio). Coloque o arquivo com palavras de 5 letras.");

            SorteiaPalavra();
            await Task.CompletedTask;
        }

        // --- Helpers para validação/normalização do arquivo de palavras ---

        // Garante que o arquivo existe e contém pelo menos minWords palavras válidas.
        // Se necessário, limpa o arquivo e escreve uma lista corrigida. Se insuficiente, escreve palavras padrão.
        public static void GarantirArquivoPalavras(string fileName, int minWords = 10)
        {
            var cleaned = new List<string>();

            if (File.Exists(fileName))
            {
                var lines = File.ReadAllLines(fileName);
                foreach (var line in lines)
                {
                    var w = CleanWord(line);
                    if (w.Length == 5) cleaned.Add(w);
                }
                cleaned = cleaned.Distinct().ToList();
            }

            // palavras padrão usadas para recuperar arquivo se necessário
            var defaults = new[]
            {
                "APOIO","TERMO","NADAR","DARDO","ANDAR","CARGO","CASAS","FRASE","VIRAR","MUNDO",
                "LIVRO","PODER","SONHO","FORCA","SORTE","AMIGO","CORPO","CASAL","PLANO","FAVOR"
            };

            // Se houver poucas palavras válidas, combine com defaults até atingir minWords
            var set = new HashSet<string>(cleaned);
            foreach (var d in defaults)
                set.Add(d);

            cleaned = set.ToList();

            // Se ainda não atingir minWords, repete defaults (provê número suficiente)
            int idx = 0;
            while (cleaned.Count < minWords)
            {
                var candidate = defaults[idx % defaults.Length];
                if (!cleaned.Contains(candidate))
                    cleaned.Add(candidate);
                idx++;
                if (idx > defaults.Length * 5) break; // proteção teórica
            }

            // escreve o arquivo com as palavras limpas (maiusculas, 5 letras)
            try
            {
                File.WriteAllLines(fileName, cleaned);
            }
            catch
            {
                // não propaga exceção aqui: chamador tratará se não conseguir carregar depois
            }
        }

        private static string CleanWord(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var s = input.Trim().ToUpperInvariant();
            s = RemoveDiacritics(s);
            var sb = new StringBuilder(5);
            foreach (var ch in s)
            {
                if (ch >= 'A' && ch <= 'Z')
                    sb.Append(ch);
                if (sb.Length == 5) break;
            }
            return sb.ToString();
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }


    }
}
