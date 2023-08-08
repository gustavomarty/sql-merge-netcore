using Contracts.Data.Data.Entities;
using Contracts.Data.Models.Dtos;

namespace Contracts.Service.Interfaces
{
    public interface IFornecedorService
    {
        Task CleanTable();
        Task Create(List<FornecedorDto> fornecedorDto);
        Task CreateBulk(List<FornecedorDto> fornecedorDto);
        Task<List<Fornecedor>> GetAll();
        Task<List<FornecedorDto>> GetNewFakes(int qtd);
        Task<List<FornecedorDto>> GetMix(int qtd, bool withChanges);
    }
}