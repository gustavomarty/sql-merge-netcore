namespace Contracts.Data.Models.Dtos
{
    public class ContratoDto
    {
        public string DocumentoFornecedor { get; set; }
        public string NumeroMaterial { get; set; }
        public string NomeClube { get; set; }
        public string Numero { get; set; }
        public decimal Preco { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }
        public string? Descricao { get; set; }
    }
}
