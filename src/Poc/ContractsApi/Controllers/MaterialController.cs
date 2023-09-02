using Contracts.Data.Data;
using Contracts.Data.Data.Entities;
using Contracts.Data.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SqlComplexOperations;
using SqlComplexOperations.Models.Enumerators;

namespace Contracts.Api.Controllers
{
    public class MaterialController : Controller
    {
        private readonly IBulkInsertBuilder _bulkInsertBuilder;
        private readonly ApplicationContext _context;

        public MaterialController(IBulkInsertBuilder bulkInsertBuilder, ApplicationContext context)
        {
            _bulkInsertBuilder = bulkInsertBuilder;
            _context = context;
        }

        [HttpPost("materials")]
        public async Task<IActionResult> Post([FromBody] List<MaterialDto> fabricas)
        {
            var dataSource = fabricas.Select(x => MaterialDtoToMaterial(x)).ToList();

            using var transaction = await _context.Database.BeginTransactionAsync();

            var mergeBuilder = _bulkInsertBuilder.Create<Material>()
                .SetDataSource(dataSource)
                .UseSnakeCaseNamingConvention()
                .SetTransaction(transaction.GetDbTransaction());

            var result = await mergeBuilder.Execute();

            await transaction.CommitAsync();

            return Ok(result);
        }

        private static Material MaterialDtoToMaterial(MaterialDto materialDto)
        {
            return new Material
            {
                Nome = materialDto.Nome,
                Numero = materialDto.Numero,
                DataAlteracao = DateTime.Now
            };
        }
    }
}
