using Contracts.Data.Data.Entities;
using Contracts.Data.Models.Dtos;

namespace Contracts.Service.Interfaces
{
    public interface IMaterialService
    {
        Task CleanTable();
        Task Create(List<MaterialDto> materialDto);
        Task CreateBulk(List<MaterialDto> materialDto);
        Task<List<Material>> GetAll();
        Task<List<MaterialDto>> GetNewFakes(int qtd);
        Task<List<MaterialDto>> GetMix(int qtd, bool withChanges);
    }
}