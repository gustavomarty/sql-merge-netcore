using Contracts.Data.Data.Entities;
using Contracts.Data.Models.Dtos;

namespace Contracts.Service.Interfaces
{
    public interface IContratoService
    {
        Task CleanTable();
        Task InsertRange(List<ContratoDto> contratoDto);
        Task Update(ContratoDto contratoDto);
        Task Upsert(List<ContratoDto> contratoDto);
        Task<List<Contrato>> GetAll();
        Task<List<ContratoDto>> GetNewFakes(int qtd);
        Task<List<ContratoDto>> GetMix(int qtd, bool withChanges);
        Task<List<ContratoDto>> GetMixAll();
    }
}