namespace Contracts.Data.Data.Entities
{
    public class Contrato
    {
        public int Id { get; set; }
        public int IdFornecedor { get; set; }
        public int IdMaterial { get; set; }
        public int IdClube { get; set; }
        public string Numero { get; set; }
        public decimal Preco { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }
        public string? Descricao { get; set; }
        public DateTime DataAlteracao { get; set; }

        public virtual Fornecedor Fornecedor { get; set; }
        public virtual Material Material { get; set; }
        public virtual Clube Clube { get; set; }
    }
}
