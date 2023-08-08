using Bulk.Models.Enumerators;
using Bulk;
using Contracts.Data.Data.Entities;
using Contracts.Data.Models.Dtos;
using Contracts.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Context = Contracts.Data.Data.ApplicationContext;

namespace Contracts.Service
{
    public class ContratoService : IContratoService
    {
        private readonly Context _context;

        public ContratoService(Context context)
        {
            _context = context;
        }

        public Task Create(List<ContratoDto> contratoDto)
        {
            throw new NotImplementedException();
        }

        public async Task CreateBulk(List<ContratoDto> contratoDto)
        {
            var dataSource = await GenerateContratoListFromContratoDtoList(contratoDto);

            using var transaction = await _context.Database.BeginTransactionAsync();

            var builder = new MergeBuilder<Contrato>()
                .SetDataSource(dataSource)
                .SetTransaction(transaction.GetDbTransaction())
                .UseSnakeCaseNamingConvention()
                .SetMergeColumns(x => new { x.IdClube, x.IdFornecedor, x.IdMaterial, x.Numero })
                .SetUpdatedColumns(x => x)
                .WithCondition(ConditionTypes.NOT_EQUAL, ConditionOperator.OR, x => new { x.Descricao, x.Fim, x.Inicio, x.Preco })
                .SetIgnoreOnIsertOperation(x => x.Id)
                .Execute();

            transaction.Commit();
        }

        public async Task<List<Contrato>> GetAll()
        {
            return await _context.Set<Contrato>().ToListAsync();
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