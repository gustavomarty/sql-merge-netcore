using Bulk.Models.Enumerators;

namespace Contracts.Data.Models.Dtos
{
    public class FornecedorDto
    {
        public string Nome { get; set; }
        public string Documento { get; set; }
        public string Cep { get; set; }
        public BulkStatus Status { get; set; }
    }
}