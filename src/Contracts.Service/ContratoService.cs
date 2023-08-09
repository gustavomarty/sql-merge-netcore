using Bulk.Models.Enumerators;
using Bulk;
using Contracts.Data.Data.Entities;
using Contracts.Data.Models.Dtos;
using Contracts.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Context = Contracts.Data.Data.ApplicationContext;
using Bogus;
using Contracts.Data.Data;
using Contracts.Service.Extensions;

namespace Contracts.Service
{
    public class ContratoService : IContratoService
    {
        private readonly Context _context;

        public ContratoService(Context context)
        {
            _context = context;
        }

        public async Task CleanTable()
        {
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE Contrato");
        }
        public async Task InsertRange(List<ContratoDto> contratoDto)
        {
            var contratos = await GenerateContratoListFromContratoDtoList(contratoDto);

            await _context.AddRangeAsync(contratos);
            await _context.SaveChangesAsync();
        }
        public async Task Update(ContratoDto contratoDto)
        {
            var contract = _context.Set<Contrato>().FirstOrDefault(c => c.Numero.Equals(contratoDto.Numero));
            if (contract != null) 
            {
                contract.Preco = contratoDto.Preco;
                contract.DataAlteracao = DateTime.Now;

                await _context.SaveChangesAsync();
            }
        }

        public async Task Upsert(List<ContratoDto> contratoDto)
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

        public async Task<List<ContratoDto>> GetMix(int qtd, bool withChanges)
        {
            var existingData = await _context.Set<Contrato>().ToListAsync();

            var quantityForGenerateData = qtd / 2;
            var quantityForExistingData = qtd / 2;

            if (existingData.Count < quantityForGenerateData)
            {
                quantityForExistingData = existingData.Count;
                quantityForGenerateData += (quantityForGenerateData - existingData.Count);
            }

            var newData = await GetNewFakeContracts(quantityForGenerateData);
            if (!newData.Any())
                return null;

            existingData.Shuffle();
            existingData = existingData.Take(quantityForExistingData).ToList();

            List<ContratoDto> result = new();
            result.AddRange(newData);
            result.AddRange(existingData.Select(x => {

                Random random = new();
                bool changeItems = withChanges && random.Next(100) < 40;

                return new ContratoDto
                {
                    Descricao = changeItems ? $"ALTERADO: {x.Descricao}" : x.Descricao,
                    DocumentoFornecedor = x.Fornecedor.Documento,
                    NomeClube = x.Clube.Nome,
                    NumeroMaterial = x.Material.Numero,
                    Numero = x.Numero,
                    Inicio = x.Inicio,
                    Fim = x.Fim,
                    Preco = x.Preco
                };
            }));

            return result;
        }

        public async Task<List<ContratoDto>> GetNewFakes(int qtd)
        {
            return await GetNewFakeContracts(qtd);
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

        private async Task<List<ContratoDto>> GetNewFakeContracts(int quantity)
        {
            var teamNames = await _context.Set<Clube>().AsNoTracking().Select(x => x.Nome).ToListAsync();
            var materialNumbers = await _context.Set<Material>().AsNoTracking().Select(x => x.Numero).ToListAsync();
            var supplierDocuments = await _context.Set<Fornecedor>().AsNoTracking().Select(x => x.Documento).ToListAsync();

            if (!teamNames.Any() || !materialNumbers.Any() || !supplierDocuments.Any())
            {
                return new List<ContratoDto>();
            }

            var faker = new Faker<ContratoDto>("pt_BR")
                .RuleFor(x => x.NomeClube, f => f.PickRandom(teamNames))
                .RuleFor(x => x.NumeroMaterial, f => f.PickRandom(materialNumbers))
                .RuleFor(x => x.DocumentoFornecedor, f => f.PickRandom(supplierDocuments))
                .RuleFor(x => x.Numero, f => f.Random.Number(10000, 99999).ToString())
                .RuleFor(x => x.Preco, f => decimal.Parse(f.Commerce.Price()))
                .RuleFor(x => x.Inicio, f => f.Date.Recent(365))
                .RuleFor(x => x.Fim, f => f.Date.Future(10));

            var response = faker.Generate(quantity);

            var responseGroup = response.GroupBy(x => new { x.NomeClube, x.NumeroMaterial, x.DocumentoFornecedor, x.Numero })
                .Select(g => g.First());

            return responseGroup.ToList();
        }
    }
}