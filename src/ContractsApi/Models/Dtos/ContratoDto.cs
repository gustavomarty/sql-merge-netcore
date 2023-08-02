namespace ContractsApi.Models.Dtos
{
    public class ContratoDto
    {
        public int IdFornecedor { get; set; }
        public int IdMaterial { get; set; }
        public int IdClube { get; set; }
        public string Numero { get; set; }
        public decimal Preco { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }
        public string? Descricao { get; set; }
    }
}
