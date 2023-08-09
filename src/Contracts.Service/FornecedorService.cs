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

        public FornecedorService(Context context)
        {
            _context = context;
        }

        public async Task CleanTable()
        {
            await _context.Database.ExecuteSqlRawAsync("delete from Fornecedor");
        }
        public async Task InsertRange(List<FornecedorDto> fornecedorDto)
        {
            var fornecedores = GenerateFornecedorListFromFornecedorDtoList(fornecedorDto);

            await _context.AddRangeAsync(fornecedores);
            await _context.SaveChangesAsync();
        }
        public async Task Update(FornecedorDto fornecedorDto)
        {
            var fornecedor = _context.Set<Fornecedor>().FirstOrDefault(m => m.Nome.Equals(fornecedorDto.Nome));
            if (fornecedor != null)
            {
                fornecedor.DataAlteracao = DateTime.Now;

                await _context.SaveChangesAsync();
            }
        }

        public async Task Upsert(List<FornecedorDto> fornecedorDto)
        {
            var dataSource = GenerateFornecedorListFromFornecedorDtoList(fornecedorDto);

            using var transaction = await _context.Database.BeginTransactionAsync();

            var builder = new MergeBuilder<Fornecedor>()
                .SetDataSource(dataSource)
                .SetTransaction(transaction.GetDbTransaction())
                .UseSnakeCaseNamingConvention()
                .SetMergeColumns(x => x.Documento)
                .SetUpdatedColumns(x => x)
                .WithCondition(ConditionTypes.NOT_EQUAL, ConditionOperator.OR, x => new { x.Cep, x.Nome })
                .SetIgnoreOnIsertOperation(x => x.Id)
                .Execute();

            transaction.Commit();
        }
        public async Task<List<Fornecedor>> GetAll()
        {
            return await _context.Set<Fornecedor>().ToListAsync();
        }
        public async Task<List<FornecedorDto>> GetNewFakes(int qtd)
        {
            return GetNewFakeSuppliers(qtd);
        }
        public async Task<List<FornecedorDto>> GetMix(int qtd, bool withChanges)
        {
            var existingData = await _context.Set<Fornecedor>().ToListAsync();

            var quantityForGenerateData = qtd / 2;
            var quantityForExistingData = qtd / 2;

            if (existingData.Count < quantityForGenerateData)
            {
                quantityForExistingData = existingData.Count;
                quantityForGenerateData += (quantityForGenerateData - existingData.Count);
            }

            var newData = GetNewFakeSuppliers(quantityForGenerateData);
            existingData.Shuffle();
            existingData = existingData.Take(quantityForExistingData).ToList();

            List<FornecedorDto> result = new();
            result.AddRange(newData);
            result.AddRange(existingData.Select(x => {

                Random random = new();
                bool changeItems = withChanges && random.Next(100) < 40;

                return new FornecedorDto
                {
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
                DataAlteracao = DateTime.Now
            }).ToList();
        }

        private List<FornecedorDto> GetNewFakeSuppliers(int quantity)
        {
            var faker = new Faker<FornecedorDto>("pt_BR")
                .RuleFor(x => x.Nome, f => f.Company.CompanyName())
                .RuleFor(x => x.Documento, f => f.Company.Cnpj(false))
                .RuleFor(x => x.Cep, f => f.Random.Number(10000000, 99999999).ToString());

            var response = faker.Generate(quantity);

            var responseGroup = response.GroupBy(x => x.Documento)
                .Select(g => g.First());

            return responseGroup.ToList();
        }
    }
}