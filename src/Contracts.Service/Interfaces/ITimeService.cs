using Contracts.Data.Data.Entities;
using Contracts.Data.Models.Dtos;

namespace Contracts.Service.Interfaces
{
    public interface ITimeService
    {
        Task Create(List<TeamDto> teamsDto);
        Task CreateBulk(List<TeamDto> teamsDto);
        Task<List<Clube>> GetAll();
        Task<List<TeamDto>> GetNewFakes(int qtd);
        Task<List<TeamDto>> GetMix(int qtd, bool withChanges);
    }
}