using Bulk;
using Bogus;
using Bogus.Extensions.Brazil;
using Bulk.Models.Enumerators;
using Contracts.Service.Extensions;
using Contracts.Data.Models.Dtos;
using Contracts.Data.Data.Entities;
using Contracts.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Context = Contracts.Data.Data.ApplicationContext;

namespace Contracts.Service
{
    public class FornecedorService : IFornecedorService
    {
        private readonly Context _context;
        private readonly IMergeBuilder _mergeBuilder;

        public FornecedorService(Context context, IMergeBuilder mergeBuilder)
        {
            _context = context;
            _mergeBuilder = mergeBuilder;
        }

        public async Task CleanTable()
        {
            await _context.Database.ExecuteSqlRawAsync("delete from Fornecedor");
        }
        public async Task Insert(Fornecedor fornecedor)
        {
            await _context.AddAsync(fornecedor);
            await _context.SaveChangesAsync();
        }
        public async Task InsertRange(List<FornecedorDto> fornecedorDto)
        {
            var fornecedores = GenerateFornecedorListFromFornecedorDtoList(fornecedorDto);

            await _context.AddRangeAsync(fornecedores);
            await _context.SaveChangesAsync();
        }
        public async Task Update(FornecedorDto fornecedorDto)
        {
            var fornecedor = await Get(fornecedorDto.Documento);
            if (fornecedor != null)
            {
                fornecedor.DataAlteracao = DateTime.Now;
                fornecedor.Nome = fornecedorDto.Nome;
                fornecedor.Cep = fornecedorDto.Cep;
                fornecedor.Status = BulkStatus.ALTERADO;

                await _context.SaveChangesAsync();
            }
        }
        public async Task Upsert(List<FornecedorDto> fornecedorDto)
        {
            var dataSource = GenerateFornecedorListFromFornecedorDtoList(fornecedorDto);

            using var transaction = await _context.Database.BeginTransactionAsync();

            var builder = await _mergeBuilder.Create<Fornecedor>()
                .SetDataSource(dataSource)
                .SetTransaction(transaction.GetDbTransaction())
                .UseSnakeCaseNamingConvention()
                .SetMergeColumns(x => x.Documento)
                .SetUpdatedColumns(x => x)
                .WithCondition(ConditionTypes.NOT_EQUAL, ConditionOperator.OR, x => new { x.Cep, x.Nome })
                .SetIgnoreOnIsertOperation(x => x.Id)
                .UseEnumStatusConfiguration(x => x.Status)
                .Execute();

            transaction.Commit();
        }
        public async Task<Fornecedor> Get(string documento)
        {
            return await _context.Set<Fornecedor>().FirstOrDefaultAsync(f => f.Documento.Equals(documento));
        }
        public async Task<List<Fornecedor>> GetAll()
        {
            return await _context.Set<Fornecedor>().ToListAsync();
        }
        public async Task<List<FornecedorDto>> GetNewFakes(int qtd)
        {
            var faker = new Faker<FornecedorDto>("pt_BR")
                .RuleFor(x => x.Status, f => BulkStatus.INSERIDO)
                .RuleFor(x => x.Nome, f => f.Company.CompanyName())
                .RuleFor(x => x.Documento, f => f.Company.Cnpj(false))
                .RuleFor(x => x.Cep, f => f.Random.Number(10000000, 99999999).ToString());

            var response = faker.Generate(qtd);

            var responseGroup = response.GroupBy(x => x.Documento)
                .Select(g => g.First());

            return responseGroup.ToList();
        }
        public async Task<List<FornecedorDto>> GetMix(int qtd, bool withChanges, bool getNewData = true)
        {
            var existingData = await _context.Set<Fornecedor>().ToListAsync();

            var quantityForGenerateData = qtd / 2;
            var quantityForExistingData = qtd / 2;

            if (existingData.Count < quantityForGenerateData)
            {
                quantityForExistingData = existingData.Count;
                quantityForGenerateData += (quantityForGenerateData - existingData.Count);
            }

            
            List<FornecedorDto> result = new();
            
            if (getNewData)
            {
                var newData = await GetNewFakes(quantityForGenerateData);
                existingData.Shuffle();
                existingData = existingData.Take(quantityForExistingData).ToList();
                result.AddRange(newData);
            }

            result.AddRange(existingData.Select(x => {

                Random random = new();
                bool changeItems = withChanges && random.Next(100) < 40;

                return new FornecedorDto
                {
                    Status = BulkStatus.INSERIDO,
                    Nome = changeItems ? $"ALTERADO: {x.Nome}" : x.Nome,
                    Documento = x.Documento,
                    Cep = x.Cep
                };
            }));

            return result;
        }
        private static List<Fornecedor> GenerateFornecedorListFromFornecedorDtoList(List<FornecedorDto> fornecedorsDto)
        {
            return fornecedorsDto.Select(x => new Fornecedor
            {
                Nome = x.Nome,
                Documento = x.Documento,
                Cep = x.Cep,
                DataAlteracao = DateTime.Now,
                Status = x.Status
            }).ToList();
        }
    }
}