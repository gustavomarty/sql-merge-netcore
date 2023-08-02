using Bulk;
using Bulk.Models.Enumerators;
using ContractsApi.Data.Entities;
using ContractsApi.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Context = ContractsApi.Data.ApplicationContext;

namespace ContractsApi.Controllers
{
    [ApiController]
    public class ContratosController : ControllerBase
    {
        private readonly Context _context;

        public ContratosController(Context context)
        {
            _context = context;
        }

        [HttpPost("contracts/teams")]
        public async Task<IActionResult> PostTeams([FromBody] List<TeamDto> teamsDto)
        {
            var dataSource = GenerateClubeListFromTeamDtoList(teamsDto);

            using var transaction = await _context.Database.BeginTransactionAsync();

            var builder = new MergeBuilder<Clube>()
                .SetDataSource(dataSource)
                .SetTransaction(transaction.GetDbTransaction())
                .UseSnakeCaseNamingConvention()
                .SetMergeColumns(x => x.Nome)
                .SetUpdatedColumns(x => x)
                .SetConditions(ConditionTypes.NOT_EQUAL, x => x.Nome)
                .SetIgnoreOnIsertOperation(x => x.Id)
                .Execute();

            transaction.Commit();

            return NoContent();
        }

        [HttpPost("contracts/materials")]
        public async Task<IActionResult> PostMaterials([FromBody] List<MaterialDto> materialsDto)
        {
            var dataSource = GenerateMaterialListFromMaterialDtoList(materialsDto);

            using var transaction = await _context.Database.BeginTransactionAsync();

            var builder = new MergeBuilder<Material>()
                .SetDataSource(dataSource)
                .SetTransaction(transaction.GetDbTransaction())
                .UseSnakeCaseNamingConvention()
                .SetMergeColumns(x => x.Numero)
                .SetUpdatedColumns(x => x)
                .SetConditions(ConditionTypes.NOT_EQUAL, x => x.Numero)
                .SetIgnoreOnIsertOperation(x => x.Id)
                .Execute();

            transaction.Commit();

            return NoContent();
        }

        [HttpPost("contracts/suppliers")]
        public async Task<IActionResult> PostSuppliers([FromBody] List<FornecedorDto> fornecedorsDto)
        {
            var dataSource = GenerateFornecedorListFromFornecedorDtoList(fornecedorsDto);

            using var transaction = await _context.Database.BeginTransactionAsync();

            var builder = new MergeBuilder<Fornecedor>()
                .SetDataSource(dataSource)
                .SetTransaction(transaction.GetDbTransaction())
                .UseSnakeCaseNamingConvention()
                .SetMergeColumns(x => x.Documento)
                .SetUpdatedColumns(x => x)
                .SetConditions(ConditionTypes.NOT_EQUAL, x => x.Documento)
                .SetIgnoreOnIsertOperation(x => x.Id)
                .Execute();

            transaction.Commit();

            return NoContent();
        }

        [HttpPost("contracts")]
        public async Task<IActionResult> Post([FromBody] List<ContratoDto> contratosDto)
        {
            var dataSource = await GenerateContratoListFromContratoDtoList(contratosDto);

            using var transaction = await _context.Database.BeginTransactionAsync();

            var builder = new MergeBuilder<Contrato>()
                .SetDataSource(dataSource)
                .SetTransaction(transaction.GetDbTransaction())
                .UseSnakeCaseNamingConvention()
                .SetMergeColumns(x => new { x.IdClube, x.IdFornecedor, x.IdMaterial, x.Numero })
                .SetUpdatedColumns(x => x)
                .SetConditions(ConditionTypes.NOT_EQUAL, x => new { x.IdClube, x.IdFornecedor, x.IdMaterial, x.Numero })
                .SetIgnoreOnIsertOperation(x => x.Id)
                .Execute();

            transaction.Commit();

            return NoContent();
        }

        private static List<Clube> GenerateClubeListFromTeamDtoList(List<TeamDto> teamsDto)
        {
            return teamsDto.Select(x => new Clube
            {
                Nome = x.Nome,
                Abreviacao = x.Abreviacao,
                Apelido = x.Apelido,
                DataAlteracao = DateTime.Now
            }).ToList();
        }

        private static List<Material> GenerateMaterialListFromMaterialDtoList(List<MaterialDto> materialsDto)
        {
            return materialsDto.Select(x => new Material
            {
                Nome = x.Nome,
                Numero = x.Numero,
                DataAlteracao = DateTime.Now
            }).ToList();
        }

        private static List<Fornecedor> GenerateFornecedorListFromFornecedorDtoList(List<FornecedorDto> fornecedorsDto)
        {
            return fornecedorsDto.Select(x => new Fornecedor
            {
                Nome = x.Nome,
                Documento = x.Documento,
                Cep = x.Cep,
                DataAlteracao = DateTime.Now
            }).ToList();
        }

        private async Task<List<Contrato>> GenerateContratoListFromContratoDtoList(List<ContratoDto> contratosDto)
        {
            var teams = await _context.Set<Clube>().Where(x => contratosDto.Select(x => x.NomeClube).Contains(x.Nome)).ToListAsync();
            var suppliers = await _context.Set<Fornecedor>().Where(x => contratosDto.Select(x => x.DocumentoFornecedor).Contains(x.Documento)).ToListAsync();
            var materials = await _context.Set<Material>().Where(x => contratosDto.Select(x => x.NumeroMaterial).Contains(x.Numero)).ToListAsync();

            var contracts = contratosDto.Select(x => new Contrato
            {
                IdClube = teams.First(y => y.Nome.Equals(x.NomeClube)).Id,
                IdFornecedor = suppliers.First(y => y.Documento.Equals(x.DocumentoFornecedor)).Id,
                IdMaterial = materials.First(y => y.Numero.Equals(x.NumeroMaterial)).Id,
                Numero = x.Numero,
                Preco = x.Preco,
                Inicio = x.Inicio,
                Fim = x.Fim,
                Descricao = x.Descricao,
                DataAlteracao = DateTime.Now
            }).ToList();

            return contracts;
        }
    }
}