using Contracts.Data.Data;
using Contracts.Data.Data.Entities;
using Contracts.Data.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using SqlComplexOperations;
using SqlComplexOperations.Models.Enumerators;

namespace Contracts.Api.Controllers
{
    [ApiController]
    public class CampeonatoController : ControllerBase
    {
        private readonly IMergeBuilder _mergeBuilder;
        private readonly ApplicationContext _context;

        public CampeonatoController(IMergeBuilder mergeBuilder, ApplicationContext context)
        {
            _mergeBuilder = mergeBuilder;
            _context = context;
        }

        [HttpPost("league")]
        public async Task<IActionResult> Post([FromBody] List<CampeonatoDto> campeonatos)
        {
            var dataSource = campeonatos.Select(x => CampeonatoDtoToCampeonato(x)).ToList();

            using var transaction = await _context.Database.BeginTransactionAsync();

            var mergeBuilder = _mergeBuilder.Create<Campeonato>("test_campeonato")
                .UsePropertyNameAttribute()
                .UseDatabaseSchema("test")
                .UseSnakeCaseNamingConvention()
                .UseStatusConfiguration(false, x => x.Status)
                .SetResponseType(ResponseType.COMPLETE)
                .SetDataSource(dataSource)
                .SetTransaction(transaction.GetDbTransaction())
                .SetMergeColumns(x => x.Nome)
                .SetUpdatedColumns(x => new { x.Pais, x.AnoFundacao, x.UpdateAt })
                .WithCondition(ConditionType.NOT_EQUAL, ConditionOperator.OR, x => new { x.Pais, x.AnoFundacao });

            var result = await mergeBuilder.Execute();

            await transaction.CommitAsync();

            return Ok(result);
        }

        private static Campeonato CampeonatoDtoToCampeonato(CampeonatoDto campeonatoDto)
        {
            return new Campeonato
            {
                Nome = campeonatoDto.Nome,
                Pais = campeonatoDto.Pais,
                AnoFundacao = campeonatoDto.AnoFundacao,
                UpdateAt = DateTime.Now
            };
        }
    }
}
