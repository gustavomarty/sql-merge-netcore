namespace Contracts.Data.Data.Entities
{
    public class Fornecedor
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Documento { get; set; }
        public string Cep { get; set; }
        public DateTime DataAlteracao { get; set; }
    }
}
