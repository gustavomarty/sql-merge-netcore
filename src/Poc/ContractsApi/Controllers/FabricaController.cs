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
    public class FabricaController : ControllerBase
    {
        private readonly IMergeBuilder _mergeBuilder;
        private readonly ApplicationContext _context;

        public FabricaController(IMergeBuilder mergeBuilder, ApplicationContext context)
        {
            _mergeBuilder = mergeBuilder;
            _context = context;
        }

        [HttpPost("factories")]
        public async Task<IActionResult> Post([FromBody] List<FabricaDto> fabricas)
        {
            var dataSource = fabricas.Select(x => FabricaDtoToFabrica(x)).ToList();

            using var transaction = await _context.Database.BeginTransactionAsync();

            var mergeBuilder = _mergeBuilder.Create<Fabrica>()
                .UseSnakeCaseNamingConvention()
                .UseDatabaseSchema("test")
                .SetResponseType(ResponseType.SIMPLE)
                .SetDataSource(dataSource)
                .SetTransaction(transaction.GetDbTransaction())
                .UseStatusConfiguration(false, x => x.BulkMergeStatus)
                .SetMergeColumns(x => x.Codigo)
                .SetUpdatedColumns(x => new { x.Nome, x.Apelido, x.QuantidadeFuncionarios, x.UpdateAt })
                .WithCondition(ConditionType.NOT_EQUAL, ConditionOperator.OR, x => new { x.Nome, x.Apelido, x.QuantidadeFuncionarios });

            var result = await mergeBuilder.Execute();

            await transaction.CommitAsync();

            return Ok(result);
        }

        private static Fabrica FabricaDtoToFabrica(FabricaDto fabricaDto)
        {
            return new Fabrica
            {
                Nome = fabricaDto.Nome,
                Apelido = fabricaDto.Apelido,
                Codigo = fabricaDto.Codigo,
                QuantidadeFuncionarios = fabricaDto.QuantidadeFuncionarios,
                BulkMergeStatus = BulkMergeStatus.INSERTED,
                UpdateAt = DateTime.Now
            };
        }
    }
}
