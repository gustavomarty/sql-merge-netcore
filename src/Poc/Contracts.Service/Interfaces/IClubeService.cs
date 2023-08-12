using Contracts.Data.Data.Entities;
using Contracts.Data.Models.Dtos;

namespace Contracts.Service.Interfaces
{
    public interface IClubeService
    {
        Task CleanTable();
        Task Insert(Clube clube);
        Task InsertRange(List<ClubeDto> clubesDto);
        Task Update(ClubeDto clubeDto);
        Task Upsert(List<ClubeDto> clubesDto);
        Task<List<Clube>> GetAll();
        Task<List<ClubeDto>> GetNewFakes(int qtd);
        Task<List<ClubeDto>> GetMix(int qtd, bool withChanges);
    }
}