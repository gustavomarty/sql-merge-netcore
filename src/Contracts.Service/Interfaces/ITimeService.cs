using Contracts.Data.Data.Entities;
using Contracts.Data.Models.Dtos;

namespace Contracts.Service.Interfaces
{
    public interface ITimeService
    {
        Task CleanTable();
        Task InsertRange(List<TeamDto> teamsDto);
        Task Update(TeamDto team);
        Task Upsert(List<TeamDto> teamsDto);
        Task<List<Clube>> GetAll();
        Task<List<TeamDto>> GetNewFakes(int qtd);
        Task<List<TeamDto>> GetMix(int qtd, bool withChanges);
    }
}