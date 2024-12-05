using SqlComplexOperations.Attributes;
using SqlComplexOperations.Models.Enumerators;

namespace Contracts.Data.Data.Entities
{
    public class Campeonato
    {
        [PropertyName("idf_campeonato")]
        public Guid Id { get; set; }

        [PropertyName("updatE_at")]
        public DateTime? UpdateAt { get; set; }

        [PropertyName("teStNoMe")]
        public string Nome { get; set; }

        [PropertyName("test_pais")]
        public string Pais { get; set; }

        [PropertyName("test_ano")]
        public int AnoFundacao { get; set; }

        [PropertyName("sts")]
        public BulkMergeStatus Status { get; set; }

        public Campeonato(string nome, string pais, int anoFundacao)
        {
            Id = Guid.NewGuid();
            Nome = nome;
            Pais = pais;
            AnoFundacao = anoFundacao;
        }

        public Campeonato()
        {
            Id = Guid.NewGuid();
        }
    }
}
