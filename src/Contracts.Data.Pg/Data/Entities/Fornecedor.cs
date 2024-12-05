using SqlComplexOperations.Models.Enumerators;

namespace Contracts.Data.Data.Entities
{
    public class Fornecedor
    {
        public Fornecedor()
        {
                
        }

        public Fornecedor(string nome, string documento, string cep) 
        {
            Nome = nome;
            Documento = documento;
            Cep = cep;
            Status = BulkMergeStatus.INSERTED;
        } 

        public int Id { get; set; }
        public string Nome { get; set; }
        public string Documento { get; set; }
        public string Cep { get; set; }
        public DateTime DataAlteracao { get; set; }
        public BulkMergeStatus Status { get; set; }
    }
}