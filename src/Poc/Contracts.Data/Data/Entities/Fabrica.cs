using Contracts.Data.Data.Entities.Base;
using SqlComplexOperations.Models.Enumerators;

namespace Contracts.Data.Data.Entities
{
    public class Fabrica : BaseEntity
    {
        public string Nome { get; set; }
        public string Apelido { get; set; }
        public string Codigo { get; set; }
        public int QuantidadeFuncionarios { get; set; }
        public BulkMergeStatus BulkMergeStatus { get; set; }

        public Fabrica(string nome, string apelido, string codigo, int quantidadeFuncionarios)
        {
            Nome = nome;
            Apelido = apelido;
            Codigo = codigo;
            QuantidadeFuncionarios = quantidadeFuncionarios;
            BulkMergeStatus = BulkMergeStatus.INSERTED;
        }

        public Fabrica()
        {
            
        }
    }
}
