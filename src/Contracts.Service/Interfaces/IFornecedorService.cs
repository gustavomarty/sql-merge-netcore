using Contracts.Data.Data.Entities;
using Contracts.Data.Models.Dtos;

namespace Contracts.Service.Interfaces
{
    public interface IFornecedorService
    {
        Task CleanTable();
        Task InsertRange(List<FornecedorDto> fornecedorDto);
        Task Update(FornecedorDto fornecedor);
        Task Upsert(List<FornecedorDto> fornecedorDto);
        Task<List<Fornecedor>> GetAll();
        Task<List<FornecedorDto>> GetNewFakes(int qtd);
        Task<List<FornecedorDto>> GetMix(int qtd, bool withChanges);
    }
}