using Contracts.Data.Data.Entities;
using Contracts.Data.Models.Dtos;

namespace Contracts.Service.Interfaces
{
    public interface IMaterialService
    {
        Task Create(List<MaterialDto> materialDto);
        Task CreateBulk(List<MaterialDto> materialDto);
        Task<List<Material>> GetAll();
    }
}
