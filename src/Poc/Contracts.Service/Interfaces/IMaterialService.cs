using Contracts.Data.Data.Entities;
using Contracts.Data.Models.Dtos;

namespace Contracts.Service.Interfaces
{
    public interface IMaterialService
    {
        Task CleanTable();
        Task InsertRange(List<MaterialDto> materialDto);
        Task Update(MaterialDto materialDto);
        Task Upsert(List<MaterialDto> materialDto);
        Task<List<Material>> GetAll();
        Task<List<MaterialDto>> GetNewFakes(int qtd);
        Task<List<MaterialDto>> GetMix(int qtd, bool withChanges);
    }
}