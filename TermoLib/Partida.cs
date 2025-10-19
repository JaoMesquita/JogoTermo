using System;
using System.Collections.Generic;

namespace TermoLib
{
    public class Partida
    {
        public string PalavraSorteada { get; set; } = string.Empty;

        public List<List<Letra>> Tentativas { get; set; } = new();

        public DateTime DataHora { get; set; } = DateTime.Now;

        public bool Venceu { get; set; }

        public int TentativasUsadas => Tentativas.Count;

        public string UltimaPalavra =>
            Tentativas.Count > 0
                ? string.Concat(Tentativas[^1].ConvertAll(l => l.Caracter))
                : string.Empty;

        public Partida() { }

        public Partida(string palavraSorteada, List<List<Letra>> tentativas, bool venceu)
        {
            PalavraSorteada = palavraSorteada;
            Tentativas = new List<List<Letra>>(tentativas);
            Venceu = venceu;
            DataHora = DateTime.Now;
        }
    }
}
