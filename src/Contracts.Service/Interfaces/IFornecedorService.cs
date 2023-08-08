using Contracts.Data.Data.Entities;
using Contracts.Data.Models.Dtos;

namespace Contracts.Service.Interfaces
{
    public interface IFornecedorService
    {
        Task Create(List<FornecedorDto> fornecedorDto);
        Task CreateBulk(List<FornecedorDto> fornecedorDto);
        Task<List<Fornecedor>> GetAll();
    }
}
