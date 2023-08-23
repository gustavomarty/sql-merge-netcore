using Contracts.Data.Data.Entities;
using Contracts.Data.Models.Dtos;
using SqlComplexOperations.Models.Enumerators;

namespace Contracts.Service.Interfaces
{
    public interface IFornecedorService
    {
        Task CleanTable();
        Task Insert(Fornecedor fornecedor);
        Task InsertRange(List<Fornecedor> fornecedores);
        Task Update(FornecedorDto fornecedorDto);
        Task Update(Fornecedor fornecedor);
        Task Upsert(List<FornecedorDto> fornecedorDto, ResponseType responseType);
        Task<Fornecedor> Get(string documento);
        Task<List<Fornecedor>> GetMany(List<string> documentos);
        Task<List<Fornecedor>> GetAll();
        Task<List<FornecedorDto>> GetNewFakes(int qtd);
        Task<List<FornecedorDto>> GetMix(int qtd, bool withChanges, bool getNewData = true);
    }
}