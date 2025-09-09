using TermoLib;

namespace TermoTest
{
    [TestClass]
    public sealed class TestTermo
    {
        [TestMethod]
        public void TestReadFile()
        {
            TestJogo();
        }
    

    [TestMethod]

        public void TestJogo()
        {
            Termo termo = new Termo();
            ImprimirJogo(termo);
            termo.ChecaPalavra("ANDAR");
            ImprimirJogo(termo);

            termo.ChecaPalavra("APOIO");
            ImprimirJogo(termo);
        }

        public void ImprimirJogo(Termo termo)
        {
            Console.WriteLine("Palavra sorteada: " + termo.palavraSorteada);

            Console.WriteLine(" T A B U L E I R O");
            foreach(var palavra in termo.tabuleiro)
            {
                foreach(var letra in palavra)
                {
                    Console.Write(letra.Caracter + ": " + letra.Cor + " |");
                }
                Console.WriteLine();
            }
            Console.WriteLine("");

            Console.WriteLine("T E C L A D O");
            foreach(var tecla in termo.teclado)
            {
                Console.Write(tecla.Key + ": " + tecla.Value + " | ");
            }
            Console.WriteLine();
        }
    }
}
