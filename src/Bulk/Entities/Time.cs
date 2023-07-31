namespace Bulk.Entities
{
    public class Time
    {
        public int Id { get; set; }
        public DateTime DataAtualizacao { get; set; }
        public string Campeonato { get; set; }
        public string Nome { get; set; }
        public int Titulos { get; set; }
        public int Participacoes { get; set; }
        public int Jogos { get; set; }
        public int Vitorias { get; set; }
        public int Derrotas { get; set; }
        public int Empates { get; set; }
    }
}
